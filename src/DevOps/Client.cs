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
        await List<Project>(new Uri("_apis/projects?api-version=7.1-preview.4", UriKind.Relative));

    public async Task<IReadOnlyList<BuildDefinition>> BuildDefinitions([NotNull]Project project) =>
        await List<BuildDefinition>(new Uri($"{project.Id}/_apis/build/Definitions", UriKind.Relative));

    public async Task<IReadOnlyList<Build>> Builds([NotNull]Project project, [NotNull]BuildDefinition definition) =>
        await List<Build>(new Uri($"{project.Id}/_apis/build/builds?definitions={definition.Id}", UriKind.Relative));

    public async Task<IReadOnlyList<Repository>> Repositories([NotNull]Project project) =>
        await List<Repository>(new Uri($"{project.Id}/_apis/git/repositories?api-version=7.1-preview.1", UriKind.Relative));

    public async Task<IReadOnlyList<ReleaseDefinition>> ReleaseDefinitions([NotNull]Project project) =>
        await List<ReleaseDefinition>(new Uri($"https://vsrm.dev.azure.com/{_org}/{project.Id}/_apis/release/definitions?api-version=7.1-preview.4"));

    public async Task<IReadOnlyList<Release>> Releases([NotNull]Project project, [NotNull]ReleaseDefinition definition) =>
        await List<Release>(new Uri($"https://vsrm.dev.azure.com/{_org}/{project.Id}/_apis/release/releases?api-version=7.1-preview.8&definitionId={definition.Id}"));

    public async Task<IReadOnlyList<Commit>> Commits([NotNull]Project project, [NotNull]Repository repository) =>
        await List<Commit>(new Uri($"{project.Id}/_apis/git/repositories/{repository.Id}/commits?api-version=7.1-preview.1", UriKind.Relative));

    public async Task<IReadOnlyList<PullRequest>> PullRequestsCompleted([NotNull]Project project, [NotNull]Repository repository, string targetBranchName) =>
        await List<PullRequest>(new Uri($"{project.Id}/_apis/git/repositories/{repository.Id}/pullrequests?searchCriteria.status=completed&searchCriteria.targetRefName=refs/heads/{targetBranchName}&api-version=7.1-preview.1", UriKind.Relative));

    public async Task<IReadOnlyList<Commit>> PullRequestCommits([NotNull]Project project, [NotNull]Repository repository, long pullRequestId) =>
        await List<Commit>(new Uri($"{project.Id}/_apis/git/repositories/{repository.Id}/pullRequests/{pullRequestId}/commits?api-version=7.1-preview.1", UriKind.Relative));

    public async Task<IReadOnlyList<Push>> Pushes([NotNull]Project project, [NotNull]Repository repository) =>
        await List<Push>(new Uri($"{project.Id}/_apis/git/repositories/{repository.Id}/pushes?api-version=7.1-preview.2", UriKind.Relative));

    public async Task<Push> PushCommits([NotNull]Project project, [NotNull]Repository repository, [NotNull]Push push, string branchName) =>
        await Get<Push>(new Uri($"{project.Id}/_apis/git/repositories/{repository.Id}/pushes/{push.PushId}?searchCriteria.refName=refs/heads/{branchName}&api-version=7.1-preview.2", UriKind.Relative));

    public async Task<Commit> LastCommit([NotNull]Project project, [NotNull]Repository repository)
    {
        var commit = await List<Commit>(new Uri(
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

    private async Task<T> Get<T>(Uri requestUri)
        where T : class
    {
        var response = await _client.GetAsync(requestUri);
        return await JsonSerializer.DeserializeAsync<T>(
            await response.Content.ReadAsStreamAsync(),
            StandardResolver.AllowPrivateExcludeNullCamelCase);
    }

    private async Task<IReadOnlyList<T>> List<T>(Uri requestUri)
        where T : class
    {
        var list = new List<T>();

        int count;
        var iteration = 0;
        do
        {
            var result = await GetPage<T>(requestUri, iteration * 100);
            list.AddRange(result.Value);

            iteration++;
            count = result.Count;
        }
        while (count == 100);

        return list;
    }

    private async Task<Response<T>> GetPage<T>(Uri requestUri, int skip = 0, int top = 100)
        where T : class
    {
        var uri = new Uri($"{requestUri}&searchCriteria.$skip={skip}&searchCriteria.$top={top}&$skip={skip}&$top={top}", UriKind.Relative);
        var response = await _client.GetAsync(uri);
        var result = await ToList<T>(response);
        return result;
    }

    private static async Task<Response<T>> ToList<T>(HttpResponseMessage response)
        where T : class
    {
        try
        {
            var result = await JsonSerializer.DeserializeAsync<Response<T>>(
                response.Content.ReadAsStream(),
                StandardResolver.AllowPrivateExcludeNullCamelCase);
            return result;
        }
        catch (JsonParsingException)
        {
            var str = await response.Content.ReadAsStringAsync();
            str = str.Replace("\"path\":\"\\\\\",", string.Empty, StringComparison.Ordinal);
            str = str.Replace(@"\\", string.Empty, StringComparison.Ordinal);
            var result =
                JsonSerializer.Deserialize<Response<T>>(
                    str,
                    StandardResolver.AllowPrivateExcludeNullCamelCase);

            return result;
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
        public int Count { get; set; }

        public List<T> Value { get; private set; } = new ();
    }
}