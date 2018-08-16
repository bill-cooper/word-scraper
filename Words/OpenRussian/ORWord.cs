using Newtonsoft.Json;

namespace RussianWordScraper.OpenRussian
{

    public class ORWord
    {
        [JsonProperty(PropertyName = "ru")]
        public string Ru { get; set; }
        [JsonProperty(PropertyName = "ruAccented")]
        public string RuAccented { get; set; }
        [JsonProperty(PropertyName = "tls")]
        public string[][] Translations { get; set; }
    }
}