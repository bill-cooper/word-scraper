using Newtonsoft.Json;

namespace RussianWordScraper.OpenRussian
{
    public class ORSentences
    {
        [JsonProperty(PropertyName = "ru")]
        public string Ru { get; set; }

        [JsonProperty(PropertyName = "tl")]
        public string Translation { get; set; }
    }
}