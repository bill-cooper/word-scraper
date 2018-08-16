using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public interface ISentenceProvider
    {
        Task<List<Sample>> GetSentences(WordForm word);
    }
}
