using RussianWordScraper.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Words
{
    public class TatoebaSentenceProvider : ISentenceProvider
    {
        public async Task<List<Sample>> GetSentences(WordForm word)
        {
            var samples = new List<Sample>();
            int pageIndex = 1;
            while (pageIndex < 50)
            {
                var page = await (new Composition { Return = new ContentSegment { Url = $"https://tatoeba.org/eng/sentences/search/page:{pageIndex}?query=%3D{word.Word}&from=rus&to=eng&orphans=no&unapproved=no&list=&has_audio=&trans_filter=limit&trans_to=und&sort=words" } }).Return.DocumentElement();
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
