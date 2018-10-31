using System.Collections.Generic;
using System.Threading.Tasks;

namespace Words
{
    public interface IWordProvider
    {
        Task<IEnumerable<WordDefinition>> GetWords(string word, bool getSamples = true);
    }
}