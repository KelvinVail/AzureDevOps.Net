namespace DevOps;

public class Deployment
{
    public long Id { get; private set; }

    public DateTime StartedOn { get; private set; }

    public ReleaseDefinition ReleaseDefinition { get; private set; }

    public ReleaseEnvironment ReleaseEnvironment { get; private set; }
}