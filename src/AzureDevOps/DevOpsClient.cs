using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureDevOps
{
    public class DevOpsClient
    {
        private readonly HttpClient _client;
        private readonly string _organization;

        public DevOpsClient(HttpClient client, string organization)
        {
            _client = client;
            _organization = organization;
        }

        public async Task<IEnumerable<Project>> Projects()
        {
            var response =
                await _client.GetAsync(
                    new Uri(
                        $"https://dev.azure.com/{_organization}/_apis/projects?api-version=6.1-preview.4"));

            var str = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GetProjectsResponse>(str);
            return result?.Value;
        }

        public async Task<IEnumerable<Repository>> Repositories(string projectId = null)
        {
            Uri uri;
            if (projectId is null)
            {
                uri =
                    new Uri(
                        $"https://dev.azure.com/{_organization}/_apis/git/repositories?api-version=6.1-preview.1");
            }
            else
            {
                uri =
                    new Uri(
                        $"https://dev.azure.com/{_organization}/{projectId}/_apis/git/repositories?api-version=6.1-preview.1");
            }

            var response =
                await _client.GetAsync(uri);

            var str = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GetRepositoriesResponse>(str);
            return result?.Value;
        }

        public async Task<Stream> ItemStream(string repositoryId, string path)
        {
            var response =
                await _client.GetAsync(
                    new Uri(
                        $"https://dev.azure.com/{_organization}/_apis/git/repositories/{repositoryId}/items?api-version=6.1-preview.1&scopePath={path}&download=true"));

            return await response.Content.ReadAsStreamAsync();
        }

        public IEnumerable<string> GetLineCharIsOn(Stream stream, long[] charOffsets)
        {
            var reader = new StreamReader(stream);
            foreach (var charOffset in charOffsets)
            {
                stream.Position = 0;
                stream.Position = charOffset;
                yield return reader.ReadLine()?.Trim();
            }
              
            reader.Close();
        }
    }
}
