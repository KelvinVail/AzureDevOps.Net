using System;

namespace AzureDevOps.PullRequestLeadTime;

public class Response
{
    public string Project { get; set; } = string.Empty;

    public string Repository { get; set; } = string.Empty;

    public long PullRequestId { get; set; }

    public DateTimeOffset PullRequestCreationDate { get; set; } = default;

    public DateTimeOffset PullRequestClosedDate { get; set; } = default;

    public string CommitId { get; set; } = string.Empty;

    public DateTimeOffset CommitDate { get; set; } = default;
}
