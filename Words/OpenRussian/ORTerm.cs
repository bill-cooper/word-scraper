using Newtonsoft.Json;

namespace RussianWordScraper.OpenRussian
{
    public class ORTerm
    {
        [JsonProperty(PropertyName = "term")]
        public string Term { get; set; }
        [JsonProperty(PropertyName = "words")]
        public ORWord[] Words { get; set; }
        [JsonProperty(PropertyName = "derivates")]
        public ORDerivates[] Derivates { get; set; }
    }
}
