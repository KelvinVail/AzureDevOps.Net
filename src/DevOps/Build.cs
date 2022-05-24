namespace DevOps;

public class Build
{
    public long Id { get; private set; }

    public string BuildNumber { get; private set; }

    public string Result { get; private set; }

    public DateTime QueueTime { get; private set; }

    public DateTime StartTime { get; private set; }

    public DateTime FinishTime { get; private set; }

    public string SourceBranch { get; private set; }

    public string SourceVersion { get; private set; }

    public Repository Repository { get; private set; }
}