using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace RecipesApi
{
    static class Git
    {
#if DEBUG
        private const string Branch = "localhost-dev";
#else
        private const string Branch = "master";
#endif
        internal static void CommitAllChanges(string repoPath, string commitMessage, string user, string email)
        {
            using var gitRepo = new Repository(repoPath);
            Commands.Stage(gitRepo, "*");
            var author = new Signature(user, email, DateTime.Now);
            var committer = author;
            var commit = gitRepo.Commit(commitMessage, author, committer);
            var options = new PushOptions
            {
                CredentialsProvider = new CredentialsHandler(
                (_, __, ___) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = Environment.GetEnvironmentVariable("git_user"),
                        Password = Environment.GetEnvironmentVariable("git_token")
                    })
            };
            gitRepo.Network.Push(gitRepo.Branches[Branch], options);
        }

        internal static void CloneRepo(ILogger logger, string repoPath)
        {
            if (Directory.Exists(repoPath))
                return;

            logger.LogInformation("Cloning repository...");
            Repository.Clone("https://github.com/thenickfish/recipes.git", repoPath, new CloneOptions
            {
                BranchName = Branch
            });
        }
    }
}
