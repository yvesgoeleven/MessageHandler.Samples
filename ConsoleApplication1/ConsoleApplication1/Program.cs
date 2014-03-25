using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Octokit.Internal;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(MainAsync(args));
        }

        static async Task MainAsync(string[] args)
        {
            var github = Build(Credentials.Anonymous);

            var newIssue = new NewIssue("Some issue")
            {
                Body = "Some body"
            };

            await github.Issue.Create("MessageHandler", "MessageHandler.Samples", newIssue);

        }

        public static GitHubClient Build(Credentials credentials)
        {
            var credentialStore = new InMemoryCredentialStore(credentials);

            var httpClient = new HttpClientAdapter();

            var connection = new Connection(
                new ProductHeaderValue("MessageHandler"),
                GitHubClient.GitHubApiUrl,
                credentialStore,
                httpClient,
                new SimpleJsonSerializer());

            var client = new GitHubClient(connection);

            return client;
        }
    }
}
