namespace DevOps;

public class Push
{
    public long PushId { get; set; }

    public DateTimeOffset Date { get; set; }

    public List<Commit> Commits { get; set; } = null!;
}
