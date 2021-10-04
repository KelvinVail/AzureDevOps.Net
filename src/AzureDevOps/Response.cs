using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AzureDevOps
{
    public class Response
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("results")]
        public List<Result> Results { get; set; }

        [JsonPropertyName("infoCode")]
        public int InfoCode { get; set; }

        [JsonPropertyName("facets")]
        public Facets Facets { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("charOffset")]
        public int CharOffset { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("column")]
        public int Column { get; set; }

        [JsonPropertyName("codeSnippet")]
        public object CodeSnippet { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Matches
    {
        [JsonPropertyName("content")]
        public List<Content> Content { get; set; }

        [JsonPropertyName("fileName")]
        public List<object> FileName { get; set; }
    }

    public class Collection
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class Version
    {
        [JsonPropertyName("branchName")]
        public string BranchName { get; set; }

        [JsonPropertyName("changeId")]
        public string ChangeId { get; set; }
    }

    public class Result
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("matches")]
        public Matches Matches { get; set; }

        [JsonPropertyName("collection")]
        public Collection Collection { get; set; }

        [JsonPropertyName("project")]
        public Project Project { get; set; }

        [JsonPropertyName("repository")]
        public Repository Repository { get; set; }

        [JsonPropertyName("versions")]
        public List<Version> Versions { get; set; }

        [JsonPropertyName("contentId")]
        public string ContentId { get; set; }
    }

    public class Facets
    {
    }
}
