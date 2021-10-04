using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps
{
    public class GetRepositoriesResponse
    {
        [JsonPropertyName("value")]
        public List<Repository> Value { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class Repository
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("project")]
        public Project Project { get; set; }

        [JsonPropertyName("defaultBranch")]
        public string DefaultBranch { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("remoteUrl")]
        public string RemoteUrl { get; set; }

        [JsonPropertyName("sshUrl")]
        public string SshUrl { get; set; }

        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; }

        [JsonPropertyName("isDisabled")]
        public bool IsDisabled { get; set; }

        //[JsonPropertyName("type")]
        //public string Type { get; set; }
    }
}
