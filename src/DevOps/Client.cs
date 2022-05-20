using System.Net;
using System.Net.Http.Headers;
using Utf8Json;
using Utf8Json.Resolvers;

namespace DevOps;

public class Client
{
    private HttpClient _client;
    private readonly string _org;
    private readonly string _pat;

    public Client(string organization, string personalAccessToken)
    {
        if (string.IsNullOrWhiteSpace(personalAccessToken))
            throw new ArgumentNullException(nameof(personalAccessToken));
        if (string.IsNullOrWhiteSpace(organization))
            throw new ArgumentNullException(nameof(organization));

        _org = organization;
        _pat = personalAccessToken;
        var clientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
        _client = new HttpClient(clientHandler);
        ConfigureClient();
    }

    public Client With(HttpClient client)
    {
        _client = client;
        ConfigureClient();
        return this;
    }

    public async Task<IReadOnlyList<Project>> Projects()
    {
        var response = await _client.GetAsync(new Uri($"https://dev.azure.com/{_org}/_apis/projects?api-version=7.1-preview.4"));
        var result = await JsonSerializer.DeserializeAsync<Response<Project>>(response.Content.ReadAsStream(), StandardResolver.AllowPrivateExcludeNullSnakeCase);

        return result.Value;
    }

    public async Task<IReadOnlyList<BuildDefinition>> BuildDefinitions(Project project)
    {
        var response = await _client.GetAsync(new Uri($"https://dev.azure.com/{_org}/{project.Id}/_apis/build/Definitions"));
        try
        {
            var result = await JsonSerializer.DeserializeAsync<Response<BuildDefinition>>(response.Content.ReadAsStream(), StandardResolver.AllowPrivateExcludeNullSnakeCase);
            return result.Value;
        }
        catch
        {
            var str = await response.Content.ReadAsStringAsync();
            str = str.Replace("\"path\":\"\\\\\",", string.Empty);
            var result =
                JsonSerializer.Deserialize<Response<BuildDefinition>>(str,
                    StandardResolver.AllowPrivateExcludeNullSnakeCase);

            return result.Value;
        }
    }

    private void ConfigureClient()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic",
            Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($":{_pat}")));
        _client.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
    }

    private class Response<T>
        where T : class
    {
        public int Count { get; set; }

        public List<T> Value { get; set; }
    }
}