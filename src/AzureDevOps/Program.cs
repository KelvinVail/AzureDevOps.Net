using System;
using System.Threading.Tasks;
using DevOps;
using Microsoft.Extensions.Configuration;

namespace AzureDevOps;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", false)
            .Build();

        using var devOps = new Client(
            config.GetSection("Organization").Value,
            config.GetSection("DevOpsKey").Value);

        var projects = await devOps.Projects();

        foreach (var project in projects)
        {
            var repo = await devOps.Repositories(project);
            foreach (var repository in repo)
            {
                var lastCommit = await devOps.LastCommit(project, repository);
                Console.WriteLine($"{project.Name}, {repository.Name}, {lastCommit.Committer.Date}, {lastCommit.Committer.Name}");
            }
        }
    }

    private static async Task ListBuilds(Client devOps, DevOps.Project project)
    {
        var buildDefs = await devOps.BuildDefinitions(project);
        foreach (var buildDefinition in buildDefs)
        {
            var builds = await devOps.Builds(project, buildDefinition);
            foreach (var build in builds)
                Console.WriteLine(
                    $"{project.Name}, {build.Repository.Name}, {build.BuildNumber}, {build.StartTime}, {(build.FinishTime - build.StartTime).TotalMinutes}");
        }
    }
}