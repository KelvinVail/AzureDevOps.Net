using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevOps.Tests;

public class GetProjectsTests
{
    private readonly HttpMessageHandlerSpy _spy = new ();
    private readonly Client _client;

    public GetProjectsTests()
    {
        var httpClient = new HttpClient(_spy);
        _client = new Client("myOrg", "PAT").With(httpClient);
        _spy.SetResponseBody(ResponseWrapper(new List<string> { ProjectResponse("1", "test") }));
    }

    [Fact]
    public async Task GetProjectsCallsCorrectUri()
    {
        var uri = new Uri("https://dev.azure.com/myOrg/_apis/projects?api-version=7.1-preview.4");

        await _client.Projects();

        _spy.AssertRequestMethod(HttpMethod.Get);
        _spy.AssertRequestUri(uri);
    }

    [Fact]
    public async Task RequestAsksForJson()
    {
        await _client.Projects();

        _spy.AssertRequestHeader("Accept", "application/json");
    }

    [Fact]
    public async Task RequestAsksForGzip()
    {
        await _client.Projects();

        _spy.AssertRequestHeader("Accept-Encoding", "gzip");
    }

    [Fact]
    public async Task PatTokenIsIncludedInHeaders()
    {
        var pat = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(":PAT"));

        await _client.Projects();

        _spy.AssertRequestHeader("Authorization", $"basic {pat}");
    }

    [Theory]
    [InlineData("myProject")]
    public async Task ProjectsAreReturned(string name)
    {
        var id = Guid.NewGuid().ToString();
        _spy.SetResponseBody(ResponseWrapper(new List<string> { ProjectResponse(id, name) }));

        var projects = await _client.Projects();

        Assert.Single(projects);
        Assert.Equal(id, projects.Single().Id);
        Assert.Equal(name, projects.Single().Name);
    }

    private static string ResponseWrapper(List<string> objects) =>
        $"{{\"count\":{objects.Count},\"value\":[{string.Join(',', objects)}]}}";

    private static string ProjectResponse(string id, string name) =>
        $"{{\"id\":\"{id}\",\"name\":\"{name}\"}}";
}