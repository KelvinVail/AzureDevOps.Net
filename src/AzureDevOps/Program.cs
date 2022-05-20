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

        var devOps = new Client(
            config.GetSection("Organization").Value,
            config.GetSection("DevOpsKey").Value);

        var projects = await devOps.Projects();

        foreach (var project in projects)
        {
            var buildDef = await devOps.BuildDefinitions(project);
            foreach (var definition in buildDef)
                Console.WriteLine($"{project}:{definition}");
        }
    }
}