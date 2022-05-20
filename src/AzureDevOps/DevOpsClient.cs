using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureDevOps
{
    public class DevOpsClient
    {
        private readonly HttpClient _client;
        private readonly string _organization;

        public DevOpsClient(string personalAccessToken, string organization)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic",
                Convert.ToBase64String(
					System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));
            _client = client;
            _organization = organization;
        }

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

        public CodeLine GetLineCharIsOn(Stream stream, long charOffset)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream);

            var charsRead = 0;
            var lineNumber = 0;

            for (int i = 0; i < charOffset; i++)
            {
                reader.Read();
            }

            var line = reader.ReadLine();
            return new CodeLine { Line = line?.Trim() };


            //while (!reader.EndOfStream)
            //{
            //    var line = reader.ReadLine();
            //    var charsOnLine = line?.Length ?? 0;
            //    lineNumber++;

            //    if (!CharOffsetIsOnThisLine(charOffset, charsRead, charsOnLine))
            //    {
            //        charsRead += charsOnLine;
            //        continue;
            //    }

            //    return new CodeLine { Line = line?.Trim(), LineNumber = lineNumber };
            //}

            //return default;
        }

        private static bool CharOffsetIsOnThisLine(long charOffset, int charsRead, int charsOnLine)
        {
            return charOffset > charsRead && charOffset <= charsRead + charsOnLine;
        }
    }
}
