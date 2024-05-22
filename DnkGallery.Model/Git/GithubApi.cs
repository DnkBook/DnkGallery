using LibGit2Sharp;
using LibGit2Sharp.Handlers;
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
    
    public async Task<RepositoryStatus> Status(string localReposPath) {
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
    
    public async Task Push(string localReposPath, string? remoteName = "origin") {
        var repo = new Repository(localReposPath);
        var remote = repo.Network.Remotes[remoteName];
        
        UsernamePasswordCredentials credentials = new UsernamePasswordCredentials {Username = "*******", Password = "******"};
        var certificateCheckHandler = new CertificateCheckHandler((certificate, valid, host) => {
            
            return valid;
        });
        var options = new PushOptions();
        options.CertificateCheck = certificateCheckHandler;
        var pushRefSpec = string.Format("+{0}:{0}","refs/remotes/github/");
        repo.Network.Push(remote, pushRefSpec, options);
    }
    
    public async Task Commit(string localReposPath, string message,Identity identity) {
        var repo = new Repository(localReposPath);
        repo.Commit(message,new Signature(identity, DateTimeOffset.Now),new Signature(identity, DateTimeOffset.Now));
    }
}
