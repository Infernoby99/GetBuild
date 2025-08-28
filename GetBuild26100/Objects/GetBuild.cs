using System.Text.Json.Serialization;

namespace GetBuild26100;

public class GetBuild
{
    public class Response
    {
        [JsonPropertyName("apiVersion")]
        public string apiVersion { get; set; }

        [JsonPropertyName("updateName")]
        public string updateName { get; set; }

        [JsonPropertyName("arch")]
        public string arch { get; set; }

        [JsonPropertyName("build")]
        public string build { get; set; }

        [JsonPropertyName("files")]
        public Dictionary<string, UupFile> Files{ get; set; }
    }

    public class Root
    {
        [JsonPropertyName("response")]
        public Response response { get; set; }

        [JsonPropertyName("jsonApiVersion")]
        public string jsonApiVersion { get; set; }
    }

    public class UupFile
    {
        [JsonPropertyName("sha1")]
        public string sha1 { get; set; }

        [JsonPropertyName("size")]
        public long size { get; set; }

        [JsonPropertyName("url")]
        public string url { get; set; }

        [JsonPropertyName("uuid")]
        public string uuid { get; set; }

        [JsonPropertyName("expire")]
        public long expire { get; set; }

        [JsonPropertyName("debug")]
        public string debug { get; set; }
    }
}