using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using Utf8Json;
using Utf8Json.Resolvers;

namespace DevOps;

public class Client : IDisposable
{
    private readonly string _org;
    private readonly string _pat;
    private readonly HttpClientHandler _clientHandler = new () { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
    private HttpClient _client;
    private bool _disposedValue;

    public Client(string organization, string personalAccessToken)
    {
        if (string.IsNullOrWhiteSpace(personalAccessToken))
            throw new ArgumentNullException(nameof(personalAccessToken));
        if (string.IsNullOrWhiteSpace(organization))
            throw new ArgumentNullException(nameof(organization));

        _org = organization;
        _pat = personalAccessToken;
        _client = ConfigureClient(new HttpClient(_clientHandler));
    }

    public Client With([NotNull]HttpClient client)
    {
        _client = ConfigureClient(client);
        return this;
    }

    public async Task<IReadOnlyList<Project>> Projects() =>
        await Get<Project>(new Uri("_apis/projects?api-version=7.1-preview.4", UriKind.Relative));

    public async Task<IReadOnlyList<BuildDefinition>> BuildDefinitions([NotNull]Project project) =>
        await Get<BuildDefinition>(new Uri($"{project.Id}/_apis/build/Definitions", UriKind.Relative));

    public async Task<IReadOnlyList<Build>> Builds([NotNull]Project project, [NotNull]BuildDefinition definition) =>
        await Get<Build>(new Uri($"{project.Id}/_apis/build/builds?definitions={definition.Id}", UriKind.Relative));

    public async Task<IReadOnlyList<Repository>> Repositories([NotNull]Project project) =>
        await Get<Repository>(new Uri($"{project.Id}/_apis/git/repositories?api-version=7.1-preview.1", UriKind.Relative));

    public async Task<IReadOnlyList<ReleaseDefinition>> ReleaseDefinitions([NotNull]Project project) =>
        await Get<ReleaseDefinition>(new Uri($"https://vsrm.dev.azure.com/{_org}/{project.Id}/_apis/release/definitions?api-version=7.1-preview.4"));

    public async Task<IReadOnlyList<Release>> Releases([NotNull]Project project, [NotNull]ReleaseDefinition definition) =>
        await Get<Release>(new Uri($"https://vsrm.dev.azure.com/{_org}/{project.Id}/_apis/release/releases?api-version=7.1-preview.8&definitionId={definition.Id}"));

    public async Task<Commit> LastCommit([NotNull]Project project, [NotNull]Repository repository)
    {
        var commit = await Get<Commit>(new Uri(
            $"{project.Id}/_apis/git/repositories/{repository.Id}/commits?api-version=7.1-preview.1&$top=1",
            UriKind.Relative));

        if (commit.Count > 0) return commit[0];

        return new Commit();
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _client.Dispose();
            _clientHandler.Dispose();
        }

        _disposedValue = true;
    }

    private async Task<IReadOnlyList<T>> Get<T>(Uri requestUri)
        where T : class
    {
        var response = await _client.GetAsync(requestUri);
        try
        {
            var result = await JsonSerializer.DeserializeAsync<Response<T>>(
                response.Content.ReadAsStream(),
                StandardResolver.AllowPrivateExcludeNullCamelCase);
            return result.Value;
        }
        catch (JsonParsingException)
        {
            var str = await response.Content.ReadAsStringAsync();
            str = str.Replace("\"path\":\"\\\\\",", string.Empty, StringComparison.Ordinal);
            var result =
                JsonSerializer.Deserialize<Response<T>>(
                    str,
                    StandardResolver.AllowPrivateExcludeNullCamelCase);

            return result.Value;
        }
    }

    private HttpClient ConfigureClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic",
            Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($":{_pat}")));
        client.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.BaseAddress = new Uri($"https://dev.azure.com/{_org}/");

        return client;
    }

    private sealed class Response<T>
        where T : class
    {
        public List<T> Value { get; private set; } = new ();
    }
}