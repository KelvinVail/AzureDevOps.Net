namespace DevOps;

public class Commit
{
    public string CommitId { get; private set; } = string.Empty;

    public Committer Committer { get; private set; } = new ();
}