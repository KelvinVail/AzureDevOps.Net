using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.Configuration;

namespace AzureDevOps
{
    class Program
    {
        private static HttpClient _client = new HttpClient();
        private static List<SummaryLineOut> _lines = new ();

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", false)
                .Build();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic",
                config.GetSection("DevOpsKey").Value);
            _client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("*/*"));


            var devOps = new DevOpsClient(_client, config.GetSection("Organization").Value);
            await using var writer = new StreamWriter(@"C:\Users\kelvi\Documents\Todos.csv");
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var repos = await devOps.Repositories();

            foreach (var repo in repos)
            {
                var result = await GetSearchResults(repo.Project.Name, repo.Name);
                if (result.Results.Count <= 0) continue;
                var lineOut = new SummaryLineOut
                {
                    ProjectName = repo.Project.Name,
                    RepositoryName = repo.Name,
                    TodoOrHackCommentCount = result.Results.SelectMany(x => x.Matches.Content).Count(),
                };

                _lines.Add(lineOut);
            }

            await csv.WriteRecordsAsync(_lines);
        }

        private static async Task<Response> GetSearchResults(string projectName, string repoName)
        {
            var searchText = string.Join(" OR ", SearchWords(new[] { "TODO", "HACK" }));
            var skip = 0;
            var top = 1000;
            return await Post(searchText, projectName, repoName, skip, top);
        }

        //private static void AddLines(Response? result)
        //{
        //    foreach (var r in result.Results)
        //    {
        //        _lines.Add(new LineOut
        //        {
        //            Project = r.Project.Name,
        //            ProjectId = r.Project.Id,
        //            Repository = r.Repository.Name,
        //            RepositoryId = r.Repository.Id,
        //            Branch = r.Versions.FirstOrDefault()?.BranchName,
        //            FileName = r.FileName,
        //            Path = r.Path,
        //            Matches = r.Matches.Content.Count,
        //            CharOffsets = string.Join("|", r.Matches.Content.Select(x => x.CharOffset)),
        //        });
        //    }
        //}

        private static async Task<Response?> Post(string searchText, string projectName, string repoName, int skip, int top)
        {
            var requestContent = new StringContent($"{{\"searchText\": \"{searchText}\"," +
                                                   $"\"$skip\": {skip}," +
                                                   $"\"$top\": {top}," +
                                                   $"\"filters\": {{\"Repository\": [\"{repoName}\"]," +
                                                   $"\"Project\":[\"{projectName}\"]}}," +
                                                   "\"$orderBy\": [" +
                                                   "{\"field\": \"path\", \"sortOrder\": \"ASC\"}" +
                                                   "]" +
                                                   "}");
            requestContent.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            var response =
                await _client.PostAsync(
                    new Uri(
                        "https://almsearch.dev.azure.com/lloydslondon/_apis/search/codesearchresults?api-version=6.1-preview.1"),
                    requestContent);

            var str = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Response>(str);
            return result;
        }

        public static IEnumerable<string> SearchWords(string[] words)
        {
            foreach (var word in words)
                foreach (var commentEscape in new List<string> { "/", "#", "/*", "<!--", "'" })
                    yield return "\\\"" + commentEscape + " " + word + "\\\"";
        }
    }
}
