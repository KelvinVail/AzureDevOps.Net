using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOps.Services.Git.Repositories
{
    public class DevOpsRepositories : DevOpsEndpoint
    {
        public DevOpsRepositories(HttpClient client, string organization)
            : base(client, organization)
        {
        }

        public async Task List()
        {

        }
    }
}
