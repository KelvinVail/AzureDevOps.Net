namespace DevOps;

public class Project
{
    public string Id { get; set; }

    public string Name { get; set; }

    public override string ToString() => Name;
}