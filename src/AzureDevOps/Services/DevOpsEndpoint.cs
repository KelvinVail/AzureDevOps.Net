using System;
using System.Net.Http;

namespace AzureDevOps.Services
{
    public abstract class DevOpsEndpoint
    {
        private readonly HttpClient _client;
        private readonly Uri _baseUri;

        protected DevOpsEndpoint(HttpClient client, string organization)
        {
            _client = client;
            _baseUri = new Uri($"https://dev.azure.com/{organization}/");
        }
    }
}
