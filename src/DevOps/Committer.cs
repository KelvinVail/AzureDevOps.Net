using System.Globalization;

namespace DevOps;

public class Committer
{
    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public DateTime Date { get; private set; } = DateTime.Parse("1970-01-01", new DateTimeFormatInfo());
}