namespace DevOps;

public class BuildDefinition
{
    public long Id { get; private set; }

    public string Name { get; private set; }

    public DateTime CreatedDate { get; private set; }

    public DateTime LastRun { get; private set; }

    public override string ToString() => Name;
}