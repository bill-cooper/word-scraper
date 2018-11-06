using System.Collections.Generic;
using System.Threading.Tasks;

namespace Words
{
    public interface IWordRepository
    {
        void ClearCache();
        Task<IEnumerable<WordDefinition>> GetAllCacheEntries();
        IEnumerable<string> GetCacheKeys();
        Task<IEnumerable<WordDefinition>> GetWords(string wordString, string wordType = "");
        Task<IEnumerable<WordDefinition>> GetWordFromCache(string wordString, string wordType = "");
        Task<IEnumerable<WordDefinition>> CacheWords(string wordString, string wordType = "");
        
    }
}