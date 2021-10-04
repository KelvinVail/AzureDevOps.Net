namespace AzureDevOps
{
    public class LineOut
    {
        public string Project { get; set; }

        public string ProjectId { get; set; }

        public string Repository { get; set; }

        public string RepositoryId { get; set; }

        public string Branch { get; set; }

        public string FileName { get; set; }

        public string Path { get; set; }

        public int Matches { get; set; }

        public string CharOffsets { get; set; }
    }
}
