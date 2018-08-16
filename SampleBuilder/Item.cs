using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleBuilder
{
    public class Item
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "word")]
        public string Word { get; set; }
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
        [JsonProperty(PropertyName = "audio")]
        public string Audio { get; set; }
        [JsonProperty(PropertyName = "read")]
        public string Read { get; set; }
        [JsonProperty(PropertyName = "pause")]
        public int Pause { get; set; }
    }
}
