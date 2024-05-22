using LibGit2Sharp;
using Octokit;
using Credentials = LibGit2Sharp.Credentials;
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
    
    public async Task Fetch(string localReposPath, string? remoteName = default, FetchOptions? options = default) {
        using var repository = new Repository(localReposPath);
        options ??= new FetchOptions();
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
            using var repository = new Repository(localReposPath);
            var retrieveStatus = repository.RetrieveStatus();
            return retrieveStatus;
        });
        return repositoryStatus;
    }
    
    public async Task<MergeResult> Pull(string localReposPath, Identity identity) {
        var mergeResult = await Task.Run(() => {
            using var repo = new Repository(localReposPath);
            var result = Commands.Pull(repo, new Signature(identity, DateTimeOffset.Now),
                new PullOptions()
                // { MergeOptions = new MergeOptions() { FastForwardStrategy = FastForwardStrategy.Default } }
            );
            return result;
        });
        return mergeResult;
    }
    
    public async Task Push(string localReposPath, Credentials credentials, string? remoteName = "origin") {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var remote = repo.Network.Remotes[remoteName];
            var options = new PushOptions {
                CredentialsProvider = (url, usernameFromUrl, types) => credentials,
            };
            //
            // var psuhRefSpaces = remote.PushRefSpecs.Select(x => x.Specification);
            repo.Network.Push(remote, repo.Refs.Head.TargetIdentifier, options);
        });
    }
    
    public async Task PullRequest(string repos,string accessToken,string title,string branch) {
        githubClient.Credentials = new Octokit.Credentials(accessToken);
        var strings = repos.Split(Path.AltDirectorySeparatorChar);
        var repository = await githubClient.Repository.Get(strings[0], strings[1]);
        var defaultBranch = await githubClient.Git.Reference.Get(repository.Id, branch);
        var featureBranch = await githubClient.Git.Reference.Create(repository.Id, new NewReference(branch, defaultBranch.Object.Sha));
        var newPullRequest = new NewPullRequest(title, featureBranch.Ref, defaultBranch.Ref);
        var pullRequest = await githubClient.PullRequest.Create(repository.Id, newPullRequest);
    }
    
    public async Task Commit(string localReposPath, string message, Identity identity) {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            var commitOptions = new CommitOptions();
            repo.Commit(message, new Signature(identity, DateTimeOffset.Now), new Signature(identity, DateTimeOffset.Now),commitOptions);
        });
    }
    public async Task Add(string localReposPath, IEnumerable<string> file) {
        await Task.Run(() => {
            var repo = new Repository(localReposPath);
            Commands.Stage(repo, file);
        });
    }
}
