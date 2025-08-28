using System.Text.Json.Serialization;

namespace GetBuild26100;

public class listid
{
    public class Build
    {
        [JsonPropertyName("title")]
        public string title { get; set; }

        [JsonPropertyName("build")]
        public string build { get; set; }

        [JsonPropertyName("arch")]
        public string arch { get; set; }

        [JsonPropertyName("created")]
        public long created { get; set; }

        [JsonPropertyName("uuid")]
        public string uuid { get; set; }
    }

    public class Response
    {
        [JsonPropertyName("apiVersion")]
        public string apiVersion { get; set; }

        [JsonPropertyName("builds")]
        public Dictionary<string, Build> builds { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("response")]
        public Response response { get; set; }

        [JsonPropertyName("jsonApiVersion")]
        public string jsonApiVersion { get; set; }
    }
}