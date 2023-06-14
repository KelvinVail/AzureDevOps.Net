namespace DevOps;

public class PullRequest
{
    public long PullRequestId { get; set; }

    public DateTimeOffset CreationDate { get; set; }

    public DateTimeOffset ClosedDate { get; set; }
}