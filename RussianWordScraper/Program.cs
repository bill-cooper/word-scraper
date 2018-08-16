using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Words;

namespace RussianWordScraper
{
    class Program
    {
        static void Main()
        {
           MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            var repo = new WordRepository();

            var wordBank = new WordBank();
            var words = await wordBank.GetWordByRank(2000);

            foreach (var word in words.Where(w => w.Length > 2))
            {
                await repo.GetWords(word);
            }

            //var wordString = JsonConvert.SerializeObject(word, Formatting.Indented);
        }
    }
}
