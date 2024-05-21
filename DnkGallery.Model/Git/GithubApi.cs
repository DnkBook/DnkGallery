using LibGit2Sharp;
using Octokit;
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
    
    public async Task Fetch(string repos, string? remoteName = default, FetchOptions? options = default) {
        using var repository = new Repository(repos);
        options ??= new FetchOptions();
        var headRemoteName = remoteName ?? repository.Head.RemoteName;
        Commands.Fetch(repository, headRemoteName, [], options, null);
    }
    
    public async Task Clone(string repos, string localPath) {
        await Task.Run(() => Repository.Clone($"{Domain}{Path.AltDirectorySeparatorChar}{repos}", localPath));
    }
    
    public async Task<MergeResult> Pull(string localReposPath, Identity identity, string? remoteName = default) {
        using var repo = new Repository(localReposPath);
        var mergeResult = await Task.Run(() => {
            var result = Commands.Pull(repo, new Signature(identity, DateTimeOffset.Now),
                new PullOptions
                    { MergeOptions = new MergeOptions() { FastForwardStrategy = FastForwardStrategy.Default } });
            return result;
        });
        return mergeResult;
    }
}
