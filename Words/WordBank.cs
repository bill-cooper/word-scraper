using RussianWordScraper.Document;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public class WordBank
    {
        public async Task<IEnumerable<string>> GetWordByRank(int count) {
            if (count % 50 != 0) throw new ArgumentException("'count' parameter must be an increment of 50");

            var words = new List<string>();

            int i = 0;
            while (i < count)
            {
                var composition = new Composition { Return = new ContentSegment { Url = $"https://en.openrussian.org/list/all?start={i}", Select = "table.wordlist" } };
                var wordTable = await composition.Return.DocumentElement();

                var hrefs = wordTable.QuerySelectorAll("td > a");
                foreach (var href in hrefs) {
                    words.Add(href.TextContent.Trim());
                }

                i += 50;
            }

            return words;
        }
    }
}
