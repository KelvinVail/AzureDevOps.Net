using System;
using System.Collections.Generic;
using System.Linq;
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

        //await ListRepos(projects, devOps);
        //await ListBuilds(devOps, projects);
        foreach (var project in projects)
        {
            var releaseDefinitions = await devOps.ReleaseDefinitions(project);
            foreach (var releaseDefinition in releaseDefinitions)
            {
                var releases = await devOps.Releases(project, releaseDefinition);
                var lastRelease = releases.OrderByDescending(x => x.CreatedOn).FirstOrDefault();
                if (lastRelease != null)
                    Console.WriteLine($"{project.Name}, {releaseDefinition.Name}, {lastRelease.CreatedOn}");
            }
        }
    }

    private static async Task ListRepos(Client devOps, IReadOnlyList<DevOps.Project> projects)
    {
        foreach (var project in projects)
        {
            var repo = await devOps.Repositories(project);
            foreach (var repository in repo)
            {
                var lastCommit = await devOps.LastCommit(project, repository);
                Console.WriteLine(
                    $"{project.Name}, {repository.Name}, {lastCommit.Committer.Date}, {lastCommit.Committer.Name}");
            }
        }
    }

    private static async Task ListBuilds(Client devOps, IReadOnlyList<DevOps.Project> projects)
    {
        foreach (var project in projects)
        {
            var buildDefs = await devOps.BuildDefinitions(project);
            foreach (var buildDefinition in buildDefs)
            {
                var builds = await devOps.Builds(project, buildDefinition);
                var latestBuild = builds.OrderByDescending(x => x.StartTime).FirstOrDefault();
                if (latestBuild != null)
                    Console.WriteLine($"{project.Name}, {latestBuild.Repository.Name}, {buildDefinition.Name}, {latestBuild.StartTime}");

                //foreach (var build in builds)
                //    Console.WriteLine(
                //        $"{project.Name}, {build.Repository.Name}, {build.BuildNumber}, {build.StartTime}, {(build.FinishTime - build.StartTime).TotalMinutes}");
            }
        }
    }
}