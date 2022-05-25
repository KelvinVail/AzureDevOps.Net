using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOps.Services.Git.Items
{
    public class GitItems
    {
        private readonly HttpClient _client;
        private readonly string _organization;

        public GitItems(HttpClient client, string organization)
        {
            _client = client;
            _organization = organization;
        }

        public async Task<Stream> Get(string repositoryId, string path)
        {
            var response =
                await _client.GetAsync(
                    new Uri(
                        $"https://dev.azure.com/{_organization}/_apis/git/repositories/{repositoryId}/items?api-version=6.1-preview.1&scopePath={path}&download=true"));

            if (!response.IsSuccessStatusCode || response.Headers?.RetryAfter != null)
                throw new Exception(await response.Content.ReadAsStringAsync());

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<CodeLine> GetCodeLine(string repositoryId, string path, long charOffset)
        {
            await using var stream = await Get(repositoryId, path);

            stream.Position = 0;
            using var reader = new StreamReader(stream);

            for (int i = 0; i < charOffset; i++)
                reader.Read();

            var line = reader.ReadLine();
            return new CodeLine { Line = line?.Trim() };
        }
    }
}
