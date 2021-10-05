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

            //var lines = await GetSummaryLineOuts(repos);
            var lines = await GetLineDetails(devOps, repos);

            await csv.WriteRecordsAsync(lines);
        }

        private static async Task<List<SummaryLineOut>> GetSummaryLineOuts(IEnumerable<Repository> repos)
        {
            List<SummaryLineOut> lines = new();
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

                lines.Add(lineOut);
            }

            return lines;
        }

        private static async Task<List<LineOut>> GetLineDetails(DevOpsClient devOps, IEnumerable<Repository> repos)
        {
            var lines = new List<LineOut>();
            foreach (var repo in repos)
            {
                var result = await GetSearchResults(repo.Project.Name, repo.Name);
                foreach (var resultResult in result.Results)
                {
                    foreach (var match in resultResult.Matches.Content)
                    {
                        var itemStream = await devOps.ItemStream(repo.Id, resultResult.Path);
                        var codeLine = devOps.GetLineCharIsOn(itemStream, match.CharOffset);
                        var lineOut = new LineOut
                        {
                            FileName = resultResult.FileName,
                            Line = codeLine.Line,
                            LineNumber = codeLine.LineNumber,
                            Path = resultResult.Path,
                            Project = repo.Project.Name,
                            Repository = repo.Name,
                        };

                        lines.Add(lineOut);
                    }
                }
            }

            return lines;
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

        private static async Task<Response> Post(string searchText, string projectName, string repoName, int skip, int top)
        {
            using var requestContent = new StringContent($"{{\"searchText\": \"{searchText}\"," +
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
