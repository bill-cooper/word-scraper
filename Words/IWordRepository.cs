using System.Collections.Generic;
using System.Threading.Tasks;

namespace Words
{
    public interface IWordRepository
    {
        void ClearCache();
        Task<IEnumerable<WordDefinition>> GetAll();
        IEnumerable<string> GetKeys();
        Task<IEnumerable<WordDefinition>> GetWords(string wordString, string wordType = "");
        void SaveWord(IEnumerable<WordDefinition> words);
    }
}