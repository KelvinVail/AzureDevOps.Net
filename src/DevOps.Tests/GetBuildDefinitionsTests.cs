using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevOps.Tests;

public class GetBuildDefinitionsTests
{
    private readonly HttpMessageHandlerSpy _spy = new ();
    private readonly Client _client;

    public GetBuildDefinitionsTests()
    {
        var httpClient = new HttpClient(_spy);
        _client = new Client("myOrg", "PAT").With(httpClient);
        _spy.SetResponseBody(ResponseWrapper(new List<string> { Response(1, "test") }));
    }

    [Theory]
    [InlineData(1, "test")]
    [InlineData(1410, "IaC")]
    public async Task ReturnsResponse(long id, string name)
    {
        _spy.SetResponseBody(ResponseWrapper(new List<string> { Response(id, name) }));

        var response = await _client.BuildDefinitions(new Project());

        Assert.Equal(id, response.Single().Id);
        Assert.Equal(name, response.Single().Name);
    }

    private static string ResponseWrapper(List<string> objects) =>
        $"{{\"count\":{objects.Count},\"value\":[{string.Join(',', objects)}]}}";

    private static string Response(long id, string name) =>
        $"{{\"id\":{id},\"name\":\"{name}\"}}";
}