using System;

namespace AzureDevOps.GetCommits;

public class Response
{
    public string CommitId { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; } = default;

    public string Project { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;
}
