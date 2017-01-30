using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreteElementAndWriteDB
{
    public class Element
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("zip")]
        public string Zip { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("typeShort")]
        public string TypeShort { get; set; }
        [JsonProperty("okato")]
        public string Okato { get; set; }
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
    }
}
