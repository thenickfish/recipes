using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

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
            using var repo = new Repository(repoPath);
            Commands.Stage(repo, "*");
            var author = new Signature(user, email, DateTime.Now);
            var committer = author;
            var commit = repo.Commit(commitMessage, author, committer);
            var options = new PushOptions
            {
                CredentialsProvider = GetCredentialsHandler()
            };
            repo.Network.Push(repo.Branches[Branch], options);
        }

        internal static void CloneRepo(ILogger logger, string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                var trackingBranch = repo.Branches[Branch];

                if (trackingBranch.IsRemote)
                {
                    var branch = repo.CreateBranch(Branch, trackingBranch.Tip);
                    repo.Branches.Update(branch, b => b.TrackedBranch = trackingBranch.CanonicalName);
                    Commands.Checkout(repo, branch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                }
                else
                {
                    Commands.Checkout(repo, trackingBranch, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                }
            }



            if (Directory.Exists(repoPath))
            {
                //reset to origin?
                using (var repo = new Repository(repoPath))
                {
                    
                    var remote = repo.Network.Remotes["origin"];
                    var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, null, "msg");
                    Commit currentCommit = repo.Head.Tip;
                    repo.Reset(ResetMode.Hard, currentCommit);
                    Pull(repoPath);
                }
                return;
            }

            logger.LogInformation("Cloning repository...");
            Repository.Clone("https://github.com/thenickfish/recipes.git", repoPath, new CloneOptions
            {
                BranchName = Branch
            });
        }

        static void Pull(string repoPath)
        {
            using (var repo = new Repository(repoPath))
            {
                PullOptions options = new PullOptions();

                options.MergeOptions = new MergeOptions();
                options.MergeOptions.FailOnConflict = true;

                options.FetchOptions = new FetchOptions();
                options.FetchOptions.CredentialsProvider = GetCredentialsHandler();

                Commands.Pull(repo, new Signature("merge", "merge", DateTime.Now), new PullOptions());
            }
        }

        private static CredentialsHandler GetCredentialsHandler() => new CredentialsHandler((_, __, ___) =>
          new UsernamePasswordCredentials()
          {
              Username = Environment.GetEnvironmentVariable("git_user"),
              Password = Environment.GetEnvironmentVariable("git_token")
          });
    }
}
