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
       // private readonly FileCache _fileCacheClient;
        private readonly StackExchangeRedisCacheClient _cacheClient;
        private readonly IWordProvider _wordProvider;
        private readonly ILogger _logger;
        public WordRepository(ILogger<WordRepository> logger, IWordProvider wordProvider)
        {
            _logger = logger;
            _logger.LogInformation("Initializing Cache client");
            //_fileCacheClient = new FileCache(@"..\app_data");
            _cacheClient = new StackExchangeRedisCacheClient(new NewtonsoftSerializer(), new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                Hosts = new RedisHost[]
                {
                    new RedisHost(){Host = "russian-word-cache.redis.cache.windows.net", Port = 6380}
                },
                Ssl = true,
                Password = "Kr+scR5X1q5NYi8uJfqVSFuHWbW+5+YSja6FP2NnCus="
            });
            _wordProvider = wordProvider;
        }

        public IEnumerable<string> GetKeys()
        {
            _logger.LogInformation("Getting all keys from word cache");
            return _cacheClient.SearchKeys("*");
        }
        public async Task<IEnumerable<WordDefinition>> GetAll()
        {
            var result = await _cacheClient.GetAllAsync<WordDefinition>(GetKeys());

            return result.Values;
        }
        public void ClearCache()
        {
            _logger.LogInformation("Clearing word cache");
            var keys = _cacheClient.SearchKeys("*");
            _cacheClient.RemoveAll(keys);
           // _fileCacheClient.Flush();
        }
        public async Task<IEnumerable<WordDefinition>> GetWords(string wordString, string wordType = "")
        {

            _logger.LogInformation($"Getting words based on word string: {wordString}");
            wordString = wordString.Trim().ToLower().RemoveStressMarks();
            List<WordDefinition> words = null;
            if (string.IsNullOrEmpty(wordType))
            {
                var cacheResult = _cacheClient.GetAll<WordDefinition>(new[] {
                    $"{wordString}-verb",
                    $"{wordString}-noun",
                    $"{wordString}-adjective",
                    $"{wordString}-adverb",
                    $"{wordString}"
                });
                words = cacheResult.Values.Where(v => v != null).ToList();
            }
            else {
                words = new List<WordDefinition>();
                var word = _cacheClient.Get<WordDefinition>($"{wordString}-{wordType}");
                if (word != null)
                    words.Add(word);
            }
            //if (words.Count() == 0)
            //{
            //    if (string.IsNullOrEmpty(wordType))
            //    {
            //        var cacheResult = _fileCacheClient.GetAll<WordDefinition>(new[] {
            //        $"{wordString}-verb",
            //        $"{wordString}-noun",
            //        $"{wordString}-adjective",
            //        $"{wordString}-adverb",
            //        $"{wordString}"
            //    });
            //        words.AddRange(cacheResult);
            //    }
            //    else
            //    {
            //        var word = _fileCacheClient.Get<WordDefinition>($"{wordString}-{wordType}");
            //        if (word != null)
            //            words.Add(word);
            //    }
            //}

            if (words.Count() == 0)
            {
                _logger.LogInformation($"Cache miss for word: {wordString}");
                try
                {
                    words = (await _wordProvider.GetWords(wordString)).ToList();
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

            SaveWord(words);

            return words;
        }

        public void SaveWord(IEnumerable<WordDefinition> words)
        {
            foreach (var word in words)
            {
                //_fileCacheClient.Add(word.Key, word);
                _cacheClient.Add(word.Key, word);
            }
        }
    }
}
