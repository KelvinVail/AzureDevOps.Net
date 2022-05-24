namespace DevOps;

public class Repository
{
    public string Id { get; private set; }

    public string Name { get; private set; }

    public string DefaultBranch { get; private set; }

    public long Size { get; private set; }
}