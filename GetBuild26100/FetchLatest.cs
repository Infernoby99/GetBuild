using System.Text.Json.Serialization;

namespace GetBuild26100;

public class FetchLatest
{
    public class Response
    {
        [JsonPropertyName("apiVersion")]
        public string apiVersion { get; set; }

        [JsonPropertyName("updateId")]
        public string updateId { get; set; }

        [JsonPropertyName("updateTitle")]
        public string updateTitle { get; set; }

        [JsonPropertyName("foundBuild")]
        public string foundBuild { get; set; }

        [JsonPropertyName("arch")]
        public string arch { get; set; }

        [JsonPropertyName("fileWrite")]
        public string fileWrite { get; set; }

        [JsonPropertyName("updateArray")]
        public List<UpdateArray> updateArray { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("response")]
        public Response response { get; set; }

        [JsonPropertyName("jsonApiVersion")]
        public string jsonApiVersion { get; set; }
    }

    public class UpdateArray
    {
        [JsonPropertyName("updateId")]
        public string updateId { get; set; }

        [JsonPropertyName("updateTitle")]
        public string updateTitle { get; set; }

        [JsonPropertyName("foundBuild")]
        public string foundBuild { get; set; }

        [JsonPropertyName("arch")]
        public string arch { get; set; }

        [JsonPropertyName("fileWrite")]
        public string fileWrite { get; set; }
    }
}