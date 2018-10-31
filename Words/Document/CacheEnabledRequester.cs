using AngleSharp;
using AngleSharp.Network;
using AngleSharp.Network.Default;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Words.Util;

namespace Words.Document
{
    public class CacheEnabledRequester : IRequester
    {
        private readonly ILogger _logger;
        private readonly IRequester _innerRequestor;
        private readonly StackExchangeRedisCacheClient _cacheClient;

        public CacheEnabledRequester(ISecretProvider secretProvider, ILogger<CacheEnabledRequester> logger)
        {
            _logger = logger;
            _innerRequestor = new HttpRequester();
            _cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                Hosts = new RedisHost[]
                {
                    new RedisHost(){Host = "russian-word-page-outputcache.redis.cache.windows.net", Port = 6379}
                },
                Ssl = false,
                Password = secretProvider.GetSecret("CachePassword/8754aa61fc9444ee957e398aa0041d8e")
            });
        }
        public async Task<IResponse> RequestAsync(IRequest request, CancellationToken cancel)
        {
            try
            {
                if ((await _cacheClient.SearchKeysAsync(request.Address.Href)).Count() != 0)
                {
                    _logger.LogInformation($"Cache hit for: {request.Address.Href}");
                    return await _cacheClient.GetAsync<CachedResponse>(request.Address.Href);
                }

                _logger.LogInformation($"Cache miss for: {request.Address.Href}");

                _logger.LogInformation($"requesting: {request.Address.Href}");
                Response response = (Response)await _innerRequestor.RequestAsync(request, cancel);
                _logger.LogInformation($"response recieved from: {request.Address.Href}");
                var cachedRequest = new CachedResponse(response);
                _logger.LogInformation($"caching entry: {request.Address.Href}");
                await _cacheClient.AddAsync(request.Address.Href, cachedRequest);

                return cachedRequest;
            }
            catch (Exception ex) {
                throw;
            }
        }

        public bool SupportsProtocol(string protocol)
        {
            return protocol.ToLower() == "http" || protocol.ToLower() == "https";
        }
    }

    public class CachedResponse : IResponse
    {
        public CachedResponse()
        { }
        public CachedResponse(IResponse innerResponse)
        {
            RawHref = innerResponse.Address.Href;
            Headers = innerResponse.Headers;
            BinaryContent = innerResponse.Content.ToByteArray();
            StatusCode = innerResponse.StatusCode;
        }

        public string RawHref
        {
            get;
            set;
        }

        [JsonIgnore]
        public Url Address
        {
            get { return new Url(RawHref); }
        }

        public IDictionary<String, String> Headers
        {
            get;
            set;
        }

        public byte[] BinaryContent
        {
            get;
            set;
        }

        [JsonIgnore]
        public Stream Content
        {
            get { return new MemoryStream(BinaryContent); }
        }
        public HttpStatusCode StatusCode
        {
            get;
            set;
        }

        void IDisposable.Dispose()
        {
            Content?.Dispose();
            Headers.Clear();
        }
    }
}
