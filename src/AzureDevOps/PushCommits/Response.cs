using System;

namespace AzureDevOps.PushCommits;

public class Response
{
    public string Project { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public long PushId { get; set; }

    public DateTimeOffset PushDate { get; set; } = default;

    public string CommitId { get; set; } = string.Empty;

    public DateTimeOffset CommitDate { get; set; } = default;
}
