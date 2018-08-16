using Newtonsoft.Json;

namespace RussianWordScraper.OpenRussian
{
    public class ORDerivates
    {
        [JsonProperty(PropertyName = "baseBare")]
        public string BaseBare { get; set; }
        [JsonProperty(PropertyName = "baseAccented")]
        public string BaseAccented { get; set; }
        [JsonProperty(PropertyName = "tl")]
        public string Translation { get; set; }
    }
}