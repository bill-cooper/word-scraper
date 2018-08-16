using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RussianWordScraper.OpenRussian;
using System.Net;
using RussianWordScraper.Util;
using RussianWordScraper.Document;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace Words
{
    public class WordProvider
    {
        private readonly ITranslator _translator;
        public WordProvider()
        {
            _translator = new Translator(new SecretProvider());
        }
        public WordProvider(ITranslator translator)
        {
            _translator = translator;
        }
        public async Task<IEnumerable<WordDefinition>> GetWords(string word, bool getSamples = true)
        {
            word = word.Trim().ToLower().RemoveStressMarks();
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://en.openrussian.org/suggestions?q={word}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var term = JsonConvert.DeserializeObject<ORTerm>(json);
                var info = new ORWordInfo();

                if (term.Words.Length > 0 && word.IsSameWord(term.Words[0].Ru))
                {
                    var orWord = term.Words[0];
                    info.Word = orWord.Ru.Trim();
                    info.StressedWord = orWord.RuAccented == string.Empty ? orWord.Ru.Trim() : WebUtility.HtmlDecode(orWord.RuAccented).Trim();

                    //var translationString = "";
                    //if (orWord.Translations.Length > 0)
                    //    foreach (var translation in orWord.Translations[0])
                    //        translationString += $";{translation.Trim()}";
                    //info.Translation = translationString.Trim();
                }
                if (term.Derivates.Length > 0)
                {
                    info.Word = word.Trim();
                    info.StressedWord = word.Trim();
                    var derivate = term.Derivates[0];
                    info.Derivate = derivate.BaseBare;
                    //if (info.Translation == string.Empty)
                    //    info.Translation = derivate.Translation.Trim();
                }

                var composition = new Composition { Return = new ContentSegment { Url = $"https://en.openrussian.org/ru/{info.Derivate}", Select = "div.page" } };
                var doc = await composition.Return.DocumentElement();

                var wordHeaders = doc.QuerySelectorAll("td > span.editable, h1");
                foreach (var header in wordHeaders)
                {
                    var wordVariant = header.TextContent.Trim();
                    if (word.IsSameWord(wordVariant))
                    {
                        info.StressedWord = wordVariant;
                        break;
                    }
                }

                var words = new List<WordDefinition>();

                var wordVersions = doc.QuerySelectorAll("div.version");
                foreach (var wordVersion in wordVersions)
                    words.Add(await CreateWordDefinition(wordVersion, info, getSamples));

                return words;
            }
        }


        private async Task<WordDefinition> CreateWordDefinition(IElement doc, ORWordInfo info, bool getSamples = true)
        {
            var wordDefinition = new WordDefinition()
            {
                Word = new WordForm()
                {
                    Word = info.Word.RemoveStressMarks(),
                    StressedWord = info.StressedWord
                }
            };

            var translationSpan = doc.QuerySelectorAll("div.translations span.editable");
            if (translationSpan.Count() > 0) {
                wordDefinition.Translations.AddRange(translationSpan.First().TextContent.Replace(", ",",").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }

            var infoDiv = doc.QuerySelectorAll("div.info");

            if (infoDiv.Count() == 0) throw new Exception("Word info counld not be found");

            GetRank(infoDiv.First(), wordDefinition);


            var infoDivContent = infoDiv.First().InnerHtml.ToLower();

            var wordDetails = infoDivContent.Substring(0, infoDivContent.IndexOf("<br>")).Trim().Split(",");

            if (wordDetails.Length == 0) throw new Exception($"No word details found for word: {wordDefinition.Word.Word}");

            wordDefinition.WordType = wordDetails[0].Trim();

            for(int i = 1; i< wordDetails.Length; i++)
                wordDefinition.Tags.Add(wordDetails[i].Trim());

            //nouns and adjectives
            var declensionDiv = doc.QuerySelectorAll("div.declension");
            if (declensionDiv.Count() > 0)
            {
                int headerRowIndex = 0;
                var formRows = declensionDiv[0].QuerySelectorAll("tr");
                for (int i = 0; i < formRows.Length; i++)
                {
                    if (formRows[i].QuerySelectorAll("th").Any() && formRows[i].ParentElement.LocalName == "thead") {
                        headerRowIndex = i;
                        continue;
                    }
                    // skip column spans above the header row.  This can happen for adjectives
                    if (formRows[i].QuerySelectorAll("td").Any() && formRows[i].QuerySelector("td").HasAttribute("colspan"))
                        continue;

                    for (int j = 1; j < formRows[i].Children.Length; j++)
                    {
                        string formDescription = "";
                        if(formRows[headerRowIndex].Children[j].QuerySelectorAll("span.long").Any())
                            formDescription = formRows[headerRowIndex].Children[j].QuerySelector("span.long").TextContent.Trim().ToLower() + " " + formRows[i].Children[0].QuerySelector("span.long").TextContent.Trim().ToLower();
                        else
                            formDescription = formRows[headerRowIndex].Children[j].TextContent.Trim().ToLower() + " " + formRows[i].Children[0].QuerySelector("span.long").TextContent.Trim().ToLower();
                        if (formRows[i].Children[j].TextContent.Trim() == "-") continue;
                        if (formRows[i].Children[j].QuerySelector("span.editable").InnerHtml.Contains("<br>"))
                        {
                            wordDefinition.WordForms.Add(new WordForm
                            {
                                Word = formRows[i].Children[j].QuerySelector("span.editable").InnerHtml.Split("<br>")[0].Trim().RemoveStressMarks(),
                                StressedWord = formRows[i].Children[j].QuerySelector("span.editable").InnerHtml.Split("<br>")[0].Trim(),
                                FormDescription = formDescription
                            });
                            wordDefinition.WordForms.Add(new WordForm
                            {
                                Word = formRows[i].Children[j].QuerySelector("span.editable").InnerHtml.Split("<br>")[1].Trim().RemoveStressMarks(),
                                StressedWord = formRows[i].Children[j].QuerySelector("span.editable").InnerHtml.Split("<br>")[1].Trim(),
                                FormDescription = formDescription + " (second form)"
                            });
                        }
                        else
                        {
                            wordDefinition.WordForms.Add(new WordForm
                            {
                                Word = formRows[i].Children[j].QuerySelector("span.editable").TextContent.Trim().RemoveStressMarks(),
                                StressedWord = formRows[i].Children[j].QuerySelector("span.editable").TextContent.Trim(),
                                FormDescription = formDescription
                            });
                        }
                    }

                }
            }

            //adjectives
            var shortFormDiv = doc.QuerySelectorAll("div.shorts");
            if (shortFormDiv.Count() > 0)
            {
                var formValues = shortFormDiv[0].QuerySelectorAll("span.editable");
                if (formValues.Count() == 4)
                {
                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[0].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[0].InnerHtml.Trim(),
                        FormDescription = "short form masculine"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[1].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[1].InnerHtml.Trim(),
                        FormDescription = "short form feminine"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[2].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[2].InnerHtml.Trim(),
                        FormDescription = "short form neuter"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[3].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[3].InnerHtml.Trim(),
                        FormDescription = "short form plural"
                    });
                }
            }

            var imperitiveFormDiv = doc.QuerySelectorAll("div.imperative");
            if (imperitiveFormDiv.Count() > 0)
            {
                var imperitiveFormValues = imperitiveFormDiv[0].QuerySelectorAll("span.editable");
                if (imperitiveFormValues.Count() == 2)
                {
                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = imperitiveFormValues[0].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = imperitiveFormValues[0].InnerHtml.Trim(),
                        FormDescription = "imperative singular"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = imperitiveFormValues[1].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = imperitiveFormValues[1].InnerHtml.Trim(),
                        FormDescription = "imperative plural"
                    });
                }
            }



            var pastFormDiv = doc.QuerySelectorAll("div.past");
            if (pastFormDiv.Count() > 0)
            {
                var formValues = pastFormDiv[0].QuerySelectorAll("span.editable");
                if (formValues.Count() == 4)
                {
                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[0].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[0].InnerHtml.Trim(),
                        FormDescription = "past masculine"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[1].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[1].InnerHtml.Trim(),
                        FormDescription = "past feminine"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[2].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[2].InnerHtml.Trim(),
                        FormDescription = "past neuter"
                    });

                    wordDefinition.WordForms.Add(new WordForm
                    {
                        Word = formValues[3].InnerHtml.Trim().RemoveStressMarks(),
                        StressedWord = formValues[3].InnerHtml.Trim(),
                        FormDescription = "past plural"
                    });
                }
            }

            var presentFutureFormDiv = doc.QuerySelectorAll("div.presfut");
            if (presentFutureFormDiv.Count() > 0)
            {
                var formRows = presentFutureFormDiv[0].QuerySelectorAll("tr");
                for (int i = 0; i < formRows.Length; i++)
                {
                    if (i == 0) continue;

                    for (int j = 1; j < formRows[i].Children.Length; j++)
                    {
                        var form = formRows[0].Children[j].TextContent.Trim().ToLower() + " " + MapConjegationType(formRows[i].Children[0].TextContent);
                        if (formRows[i].Children[j].TextContent == "-") continue;
                        wordDefinition.WordForms.Add(new WordForm
                        {
                            Word = formRows[i].Children[j].TextContent.Trim().RemoveStressMarks(),
                            StressedWord = formRows[i].Children[j].TextContent.Trim(),
                            FormDescription = form
                        });
                    }
                }
            }

            if (getSamples)
            {
                wordDefinition = ScrapeAudio(wordDefinition, doc);

                await GetSamples(wordDefinition);
            }

            GetRelatedWords(doc, wordDefinition);

            return wordDefinition;
        }

        private void GetRelatedWords(IElement doc, WordDefinition wordDefinition)
        {
            if (doc.QuerySelectorAll("a.verb-partner").Any()) {
                wordDefinition.RelatedWords.Add(new RelatedWord {
                    Word = doc.QuerySelectorAll("a.verb-partner").First().TextContent.Trim().RemoveStressMarks(),
                    Relationship = "partner verb"
                });
            }
            if (doc.QuerySelectorAll("span.adjective-adverb").Any())
            {
                wordDefinition.RelatedWords.Add(new RelatedWord
                {
                    Word = doc.QuerySelectorAll("span.adjective-adverb").First().TextContent.Trim().RemoveStressMarks(),
                    Relationship = "adverb"
                });
            }
            foreach (var relatedWordElementDiv in doc.QuerySelectorAll(".relateds2"))
            {
                foreach (var relatedWordElement in relatedWordElementDiv.QuerySelectorAll("a"))
                {

                    //skip related words that do not have a translation
                    if (relatedWordElement.ParentElement.QuerySelector("p").TextContent.Trim() == string.Empty)
                        continue;

                    var relatedWord = relatedWordElement.TextContent.Trim().RemoveStressMarks();

                    wordDefinition.RelatedWords.Add(new RelatedWord { Word = relatedWord });
                }
            }
        }

        private void GetRank(IElement info, WordDefinition wordDefinition)
        {
            try
            {
                var wordRank = info.QuerySelectorAll(".word-rank");
                if (wordRank.Count() == 0) return;
                Match m = Regex.Match(wordRank.First().TextContent, @".*\(#(?<rank>[0-9]*)\)");
                if (m.Success)
                {
                    var rank = m.Groups["rank"].Value;
                    wordDefinition.Rank = Int32.Parse(rank);
                }
            }
            catch(Exception ex) {
                Console.WriteLine($"Exception: Could not parse the word rank: {ex.Message}");
            }
        }

        private async Task GetSamples(WordDefinition wordDefinition)
        {
            var audioSourceDictionary = new Dictionary<string, List<AudioSource>>();
            var sampleDictionary = new Dictionary<string, List<Sample>>();

            foreach (var word in wordDefinition.WordForms.Select(wf => wf.Word).Union(new[] { wordDefinition.Word.Word }).Distinct())
            {
                if (word.Split(" ").Count() > 1) continue; //skip forms with multiple words
                var wordAudioElements = await (new Composition { Return = new ContentSegment { Url = $"https://forvo.com/word/{word}/#ru", Select = "span.play" } }).Return.DocumentElement();
                var audioSources = new List<AudioSource>();
                foreach (var element in wordAudioElements)
                {
                    var onclick = element.GetAttribute("onclick");
                    var onclickParts = onclick.Split(',');
                    if (onclickParts.Count() >= 5)
                    {
                        var source = Encoding.UTF8.GetString(Convert.FromBase64String(onclickParts[4].Trim(new[] { '\'', '"' })));
                        source = $"https://audio00.forvo.com/audios/mp3/{source}";

                        Uri uri;
                        if (Uri.TryCreate(source, UriKind.Absolute, out uri))
                        {
                            audioSources.Add(new AudioSource { Uri = uri });
                        }
                    }
                }
                audioSourceDictionary.Add(word, audioSources);

                var phraseElements = await (new Composition { Return = new ContentSegment { Url = $"https://forvo.com/search/{word}/ru/", Select = "li.list-phrases" } }).Return.DocumentElement();
                var phraseAudioElements = phraseElements.QuerySelectorAll("span.play");

                var phrases = new List<Sample>();
                foreach (var element in phraseAudioElements)
                {
                    var phraseText = element.GetAttribute("title").Replace("Listen", "").Replace("pronunciation", "").Trim();
                    if (!phraseText.ToLower().Contains(word))
                        continue;
                    //skip quotes
                    if (phraseText.Contains("[") || phraseText.Contains("("))
                        continue;

                        var onclick = element.GetAttribute("onclick");
                    var onclickParts = onclick.Split(',');
                    if (onclickParts.Count() >= 3)
                    {
                        var source = Encoding.UTF8.GetString(Convert.FromBase64String(onclickParts[1].Trim(new[] { '\'', '"' })));
                        source = $"https://audio00.forvo.com/phrases/mp3/{source}";

                        Uri uri;
                        if (Uri.TryCreate(source, UriKind.Absolute, out uri))
                        {
                            phrases.Add(new Sample
                            {
                                AudioSource = new AudioSource { Uri = uri },
                                SampleText = phraseText,
                                Translation = await _translator.Translate(phraseText)
                            });
                        }
                    }
                }
                sampleDictionary.Add(word, phrases);
            }

            foreach(var wordForm in wordDefinition.WordForms)
            {
                if(audioSourceDictionary.ContainsKey(wordForm.Word))
                    wordForm.AudioSources.AddRange(audioSourceDictionary[wordForm.Word]);
                if (sampleDictionary.ContainsKey(wordForm.Word))
                    wordForm.Samples.AddRange(sampleDictionary[wordForm.Word]);
            }
            if (audioSourceDictionary.ContainsKey(wordDefinition.Word.Word))
                wordDefinition.Word.AudioSources.AddRange(audioSourceDictionary[wordDefinition.Word.Word]);
            if (sampleDictionary.ContainsKey(wordDefinition.Word.Word))
                wordDefinition.Word.Samples.AddRange(sampleDictionary[wordDefinition.Word.Word]);
        }
        private WordDefinition ScrapeAudio(WordDefinition definition, IElement doc)
        {
            var audios = doc.QuerySelectorAll("audio");
            if (audios.Count() > 0)
            {
                definition.Word.AudioSources = new List<AudioSource> {
                     new AudioSource{
                          Uri = new Uri(audios[0].Attributes["src"].Value)
                     }
                };
            }
            return definition;
        }
        private string MapConjegationType(string indicator)
        {
            if (indicator.ToLower() == "я")
                return "first person singular";
            if (indicator.ToLower() == "ты")
                return "second person singular";
            if (indicator.ToLower() == "он/она́/оно́")
                return "third person singular";
            if (indicator.ToLower() == "мы")
                return "first person plural";
            if (indicator.ToLower() == "вы")
                return "second person plural";
            if (indicator.ToLower() == "они́")
                return "third person plural";
            return "unknown";
        }
    }
}
