using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
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

        while (true)
        {
            Console.WriteLine("Type 'repos' to list all repos.");
            Console.WriteLine("Type 'builds' to list latest builds.");
            Console.WriteLine("Type 'releases' to list latest releases.");

            var input = Console.ReadLine();

            if (input == "repos") await ListRepos(devOps);
            if (input == "builds") await ListBuilds(devOps);
            if (input == "releases") await ListReleases(devOps);
        }
    }

    private static async Task ListReleases(Client devOps)
    {
        var projects = await devOps.Projects();
        var project = projects.Single(x => x.Name == "DevOps");
        var repos = await devOps.Repositories(project);
        var repo = repos.Single(x => x.Name == "EVMM");

        await ListPushCommits(devOps, project, repo);

        //await ListPullRequestCommits(devOps, project, repo);
        //await ListRepos(projects, devOps);
        //await ListBuilds(devOps, projects);
        //await ListReleases(projects, devOps);
        //await ListCommits(projects, devOps);
    }

    private static async Task ListPushCommits(Client devOps, DevOps.Project project, DevOps.Repository repo)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
        };

        await using var stream = File.Open(@"C:\Users\kelvi\OneDrive\Documents\Work Documents\" + $"{project.Name}-{repo.Name}-{DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss")}" + ".csv", FileMode.Append);
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, csvConfig);
        csv.WriteHeader<PushCommits.Response>();
        await csv.NextRecordAsync();

        var pushes = await devOps.Pushes(project, repo);
        foreach (var push in pushes)
        {
            var commits = await devOps.PushCommits(project, repo, push, "develop");
            var records = commits.Commits.Select(x => new PushCommits.Response()
            {
                Project = project.Name,
                Repository = repo.Name,
                PushId = push.PushId,
                PushDate = push.Date,
                CommitId = x.CommitId,
                CommitDate = x.Committer.Date,
            });

            await csv.WriteRecordsAsync(records);
        }
    }

    private static async Task ListPullRequestCommits(Client devOps, DevOps.Project project, DevOps.Repository repo)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
        };

        await using var stream = File.Open(@"C:\Users\kelvi\OneDrive\Documents\Work Documents\" + $"{project.Name}-{repo.Name}-{DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss")}" + ".csv", FileMode.Append);
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, csvConfig);
        csv.WriteHeader<PullRequestLeadTime.Response>();

        var pullRequests = await devOps.PullRequestsCompleted(project, repo, "master");
        foreach (var pullRequest in pullRequests)
        {
            var commits = await devOps.PullRequestCommits(project, repo, pullRequest.PullRequestId);
            var records = commits.Select(x => new PullRequestLeadTime.Response()
            {
                Project = project.Name,
                Repository = repo.Name,
                PullRequestId = pullRequest.PullRequestId,
                PullRequestCreationDate = pullRequest.CreationDate,
                PullRequestClosedDate = pullRequest.ClosedDate,
                CommitId = x.CommitId,
                CommitDate = x.Committer.Date,
            });

            await csv.WriteRecordsAsync(records);
        }
    }

    private static async Task ListCommits(IReadOnlyList<DevOps.Project> projects, Client devOps)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false,
        };

        await using var stream = File.Open(@"C:\Users\kelvi\OneDrive\Documents\Work Documents\commits.csv", FileMode.Append);
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, csvConfig);
        foreach (var project in projects)
        {
            var repos = await devOps.Repositories(project);
            foreach (var repo in repos)
            {
                var commits = await devOps.Commits(project, repo);
                var records = commits.Select(x => new GetCommits.Response()
                {
                    CommitId = x.CommitId,
                    Date = x.Committer.Date,
                    Project = project.Name,
                    Repository = repo.Name,
                });

                await csv.WriteRecordsAsync(records);
            }
        }
    }

    private static async Task ListReleases(IReadOnlyList<DevOps.Project> projects, Client devOps)
    {
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

    private static async Task GetRepos(Client devOps, IReadOnlyList<DevOps.Project> projects)
    {
        var projects = await devOps.Projects();
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

    private static async Task ListBuilds(Client devOps)
    {
        var projects = await devOps.Projects();
        foreach (var project in projects)
        {
            var buildDefs = await devOps.BuildDefinitions(project);
            foreach (var buildDefinition in buildDefs)
            {
                var builds = await devOps.Builds(project, buildDefinition);
                var latestBuild = builds.OrderByDescending(x => x.StartTime).FirstOrDefault();
                if (latestBuild != null)
                    Console.WriteLine($"{project.Name}, {latestBuild.Repository.Name}, {buildDefinition.Name}, {latestBuild.StartTime}");
            }
        }
    }
}