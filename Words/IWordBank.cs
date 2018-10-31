using System.Collections.Generic;
using System.Threading.Tasks;

namespace Words
{
    public interface IWordBank
    {
        Task<IEnumerable<string>> GetWordByRank(int count);
    }
}