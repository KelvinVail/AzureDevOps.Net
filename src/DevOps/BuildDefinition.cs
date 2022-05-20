namespace DevOps;

public class BuildDefinition
{
    public long Id { get; set; }

    public string Name { get; set; }

    public override string ToString() => Name;
}