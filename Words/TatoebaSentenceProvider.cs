
using Microsoft.Extensions.Logging;
using RussianWordScraper.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Words
{
    public class TatoebaSentenceProvider : ISentenceProvider
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        public TatoebaSentenceProvider(ILogger<TatoebaSentenceProvider> logger, IServiceProvider serviceProvider) {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        public async Task<List<Sample>> GetSentences(WordForm word)
        {
            _logger.LogInformation($"Getting sentences from Tatoeba for word: {word.Word}");
            var samples = new List<Sample>();
            int pageIndex = 1;
            while (pageIndex < 50)
            {
                var contentSegment = _serviceProvider.GetRequiredService<ContentSegment>();
                contentSegment.Url = $"https://tatoeba.org/eng/sentences/search/page:{pageIndex}?query=%3D{word.Word}&from=rus&to=eng&orphans=no&unapproved=no&list=&has_audio=&trans_filter=limit&trans_to=und&sort=words";
                var page = await (new Composition { Return = contentSegment }).Return.DocumentElement();
                if (page[0].QuerySelector("p.error") != null)
                    break;
                var sentenceDivs = page.QuerySelectorAll("div.sentence-and-translations");
                if (sentenceDivs == null)
                    throw new Exception("Could not find sentence divs using selector 'div.sentence-and-translations'");

                foreach (var sentenceDiv in sentenceDivs)
                {
                    var nativeSentenceDiv = sentenceDiv.QuerySelector("div.sentence > div.text");
                    if (nativeSentenceDiv == null)
                        throw new Exception("Could not find native sentence div using selector 'div.sentence > div.text'");
                    var translatedSentenceDiv = sentenceDiv.QuerySelector("div.translation > div.text");
                    if (translatedSentenceDiv == null)
                        continue; //there is not translation provided for this sentence

                    var sample = new Sample { SampleText = nativeSentenceDiv.TextContent.Trim(), Translation = translatedSentenceDiv.TextContent.Trim() };
                    //only add this sentence if it is not a duplicate
                    if(!samples.Any( s => s.Key == sample.Key))
                        samples.Add(sample);

                }
                pageIndex++;
            }

            return samples;
        }
    }
}
