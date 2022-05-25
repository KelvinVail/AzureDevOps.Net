namespace AzureDevOps.Services.Git.Items
{
    public class GetLineCommand
    {
        public string RepositoryId { get; set; }

        public string Path { get; set; }

        public long CharOffset { get; set; }
    }
}
