using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RussianWordScraper.Document;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public class WordBank : IWordBank
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        public WordBank(ILogger<WordProvider> logger, IServiceProvider serviceProvider) {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task<IEnumerable<string>> GetWordByRank(int count) {
            if (count % 50 != 0) throw new ArgumentException("'count' parameter must be an increment of 50");

            var words = new List<string>();

            int i = 0;
            _logger.LogInformation("Starting to pull word list from openrussian.org");
            while (i < count)
            {

                var contentSegment = _serviceProvider.GetRequiredService<ContentSegment>();
                contentSegment.Url = $"https://en.openrussian.org/list/all?start={i}";
                contentSegment.Select = "table.wordlist";
                var composition = new Composition { Return = contentSegment };
                var wordTable = await composition.Return.DocumentElement();

                var hrefs = wordTable.QuerySelectorAll("td > a");
                foreach (var href in hrefs) {
                    words.Add(href.TextContent.Trim());
                }

                i += 50;
            }

            _logger.LogInformation("Words pulled from from openrussian.org");
            return words;
        }
    }
}
