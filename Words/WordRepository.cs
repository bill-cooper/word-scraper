using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RussianWordScraper.Util;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public class WordRepository : IWordRepository
    {
        private readonly StackExchangeRedisCacheClient _cacheClient;
        private readonly IWordProvider _wordProvider;
        private readonly ILogger _logger;
        public WordRepository(ILogger<WordRepository> logger, IWordProvider wordProvider)
        {
            _logger = logger;
            _logger.LogInformation("Initializing Cache client");
            _cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                Hosts = new RedisHost[]
                {
                    new RedisHost(){Host = "russian-word-cache.redis.cache.windows.net", Port = 6380}
                },
                Ssl = true,
                Password = "Kr+scR5X1q5NYi8uJfqVSFuHWbW+5+YSja6FP2NnCus=",
                ConnectTimeout = 50000
            });
            _wordProvider = wordProvider;
        }

        public IEnumerable<string> GetCacheKeys()
        {
            _logger.LogInformation("Getting all keys from word cache");
            return _cacheClient.SearchKeys("*");
        }
        public async Task<IEnumerable<WordDefinition>> GetAllCacheEntries()
        {
            var result = await _cacheClient.GetAllAsync<WordDefinition>(GetCacheKeys());
            return result.Values;
        }
        public void ClearCache()
        {
            _logger.LogInformation("Clearing word cache");
            var keys = _cacheClient.SearchKeys("*");
            _cacheClient.RemoveAll(keys);
        }

        public async Task<IEnumerable<WordDefinition>> GetWordFromCache(string wordString, string wordType = "")
        {
            if (string.IsNullOrEmpty(wordType))
            {
                var cacheResult = await _cacheClient.GetAllAsync<WordDefinition>(new[] {
                    $"{wordString}-verb",
                    $"{wordString}-noun",
                    $"{wordString}-adjective",
                    $"{wordString}-adverb",
                    $"{wordString}"
                });
                return cacheResult.Values.Where(v => v != null).ToList();
            }
            else
            {
                var words = new List<WordDefinition>();
                var word = _cacheClient.Get<WordDefinition>($"{wordString}-{wordType}");
                if (word != null)
                    words.Add(word);

                return words;
            }
        }
        public async Task<IEnumerable<WordDefinition>> CacheWords(string wordString, string wordType = "") {
            _logger.LogInformation($"Request to cache words based on word string: {wordString}");
            return await GetWords(wordString, wordType);
        }
        public async Task<IEnumerable<WordDefinition>> GetWords(string wordString, string wordType = "")
        {
            _logger.LogInformation($"Getting words based on word string: {wordString}");
            wordString = wordString.Trim().ToLower().RemoveStressMarks();
            var words = await GetWordFromCache(wordString, wordType);


            if (words.Count() == 0)
            {
                _logger.LogInformation($"Cache miss for word: {wordString}");
                try
                {
                    _logger.LogInformation($"Get words from wordprovider for: {wordString}");
                    var providerWordList = await _wordProvider.GetWords(wordString);
                    if (providerWordList != null && providerWordList.Count() > 0)
                    {
                        words = providerWordList.ToList();
                        _logger.LogInformation($"Retrieved {words.Count()} words from wordprovider for: {wordString}");
                        SaveWord(words);
                    }
                    else
                    {
                        _logger.LogInformation($"No words retrieved from wordprovider for: {wordString}");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return null;
                }
            }
            else {
                _logger.LogInformation($"Cache hit for word: {wordString}");
            }


            return words;
        }

        private void SaveWord(IEnumerable<WordDefinition> words)
        {
            foreach (var word in words)
            {
                _logger.LogInformation($"Saving word to cache: {word.Word}");
                _cacheClient.Add(word.Key, word);
            }
        }
    }
}
