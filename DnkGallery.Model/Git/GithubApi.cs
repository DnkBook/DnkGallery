using LibGit2Sharp;
using Octokit;
using Branch = LibGit2Sharp.Branch;
using Repository = LibGit2Sharp.Repository;
using Signature = LibGit2Sharp.Signature;

namespace DnkGallery.Model.Github;

public class GithubApi(GitHubClient githubClient) : IGitApi {
    private const string Domain = "https://github.com";
    
    public async Task<IReadOnlyList<RepositoryContent>?> GetReposContent(string repos, string? path = "/") {
        var strings = repos.Split(Path.AltDirectorySeparatorChar);
        var response = await githubClient.Repository.Content.GetAllContents(strings[0], strings[1], path);
        return response;
    }
    
    public async Task<byte[]?> GetReposRawContent(string repos, string? path = "/") {
        var strings = repos.Split(Path.AltDirectorySeparatorChar);
        var rawContent = await githubClient.Repository.Content.GetRawContent(strings[0], strings[1], path);
        return rawContent;
    }
    
    public async Task Fetch(string localReposPath, string? remoteName = default) {
        var repository = new Repository(localReposPath);
        var options = new FetchOptions();
        var headRemoteName = remoteName ?? repository.Head.RemoteName;
        Commands.Fetch(repository, headRemoteName, [], options, null);
    }
    
    public async Task Clone(string repos, string localPath) {
        await Task.Run(() => Repository.Clone($"{Domain}{Path.AltDirectorySeparatorChar}{repos}", localPath));
    }
    
    public bool CheckWorkDir(string localReposPath) {
        try {
            return Directory.Exists(localReposPath) && Repository.IsValid(localReposPath);
        } catch {
            return false;
        }
    }
    
    public async Task<RepositoryStatus?> Status(string localReposPath) {
        if (!CheckWorkDir(localReposPath)) {
            return null;
        }
        
        var repositoryStatus = await Task.Run(() => {
            var repository = new Repository(localReposPath);
            var retrieveStatus = repository.RetrieveStatus();
            return retrieveStatus;
        });
        return repositoryStatus;
    }
    
    public async Task<MergeResult> Pull(string localReposPath, string userName, string? email = default) {
        var mergeResult = await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var result = Commands.Pull(repo,
                new Signature(new Identity(userName, email ?? userName), DateTimeOffset.Now),
                new PullOptions()
                // { MergeOptions = new MergeOptions() { FastForwardStrategy = FastForwardStrategy.Default } }
            );
            return result;
        });
        return mergeResult;
    }
    
    public async Task Push(string localReposPath, string userName, string accessToken, string? remoteName = "origin") {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var remote = repo.Network.Remotes[remoteName];
            var options = new PushOptions {
                CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials() {
                    Username = userName,
                    Password = accessToken
                },
            };
            if (repo.Head.IsTracking) {
                repo.Network.Push(remote, repo.Head.CanonicalName, options);
            } else {
                var remoteBranch = repo.Head.CanonicalName.Replace("heads", $"remotes/{remoteName}");
                repo.Network.Push(remote, repo.Head.CanonicalName, options);
                var trackedBranch = repo.Branches.FirstOrDefault(x => x.UpstreamBranchCanonicalName == repo.Head.CanonicalName)?.CanonicalName ?? remoteBranch;
                repo.Branches.Update(repo.Head,
                    b => b.TrackedBranch = trackedBranch);
            }
            
        });
    }
    
    public async Task PullRequest(string repos, string localReposPath, string accessToken, string title, string branch, string? toBranch = "main") {
        var repo = new Repository(localReposPath);
        var repoBranch = repo.Branches[branch];
        var baseBranch = repo.Branches[toBranch];
        
        githubClient.Credentials = new Octokit.Credentials(accessToken);
        var strings = repos.Split(Path.AltDirectorySeparatorChar);
        var repository = await githubClient.Repository.Get(strings[0], strings[1]);
        
        
        var defaultBranch = await githubClient.Git.Reference.Get(repository.Id, baseBranch.CanonicalName);
        var featureBranch = await githubClient.Git.Reference.Get(repository.Id, repoBranch.CanonicalName);
        
        var newPullRequest = new NewPullRequest(title, featureBranch.Ref, defaultBranch.Ref);
        var pullRequest = await githubClient.PullRequest.Create(repository.Id, newPullRequest);
    }
    
    public async Task Commit(string localReposPath, string message, string userName, string? email = default) {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var commitOptions = new CommitOptions();
            repo.Commit(message, new Signature(new Identity(userName, email ?? userName), DateTimeOffset.Now),
                new Signature(userName, email ?? userName, DateTimeOffset.Now), commitOptions);
        });
    }
    
    public async Task<Branch> Checkout(string localReposPath, Branch branch) {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var checkout = Commands.Checkout(repo, branch);
            return checkout;
        });
    }
    
    public async Task<ICommitLog> BeingPushedCommits(string localReposPath) {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var trackingBranch = repo.Head.TrackedBranch;
            var log = repo.Commits.QueryBy(new CommitFilter()
                { IncludeReachableFrom = repo.Head.Tip.Id, ExcludeReachableFrom = trackingBranch?.Tip?.Id });
            return log;
        });
    }
    
    public async Task<Branch> Branch(string localReposPath) {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            return repo.Head;
        });
    }
    
    public async Task<IQueryableCommitLog> Commits(string localReposPath) {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var log = repo.Commits;
            return log;
        });
    }
    
    public async Task<BranchCollection> Branches(string localReposPath) {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            return repo.Branches;
        });
    }
    
    
    public async Task<Branch> CreateBranch(string localReposPath, string branchName, string? remoteName = "origin") {
        return await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var branch = repo.CreateBranch(branchName);
            
            
            var remoteBranch = branch.CanonicalName.Replace("heads", $"remotes/{remoteName}");
            var trackedBranch = repo.Branches.FirstOrDefault(x => x.CanonicalName == remoteBranch)?.CanonicalName;
            if (trackedBranch is not null) {
                repo.Branches.Update(branch,
                    b => b.TrackedBranch = trackedBranch);
            }

            return branch;
        });
    }
    
    public async Task Add(string localReposPath, IEnumerable<string> file) {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            Commands.Stage(repo, file);
        });
    }
}
