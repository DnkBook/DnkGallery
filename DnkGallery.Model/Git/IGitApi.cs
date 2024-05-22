using LibGit2Sharp;
using Octokit;
using Credentials = LibGit2Sharp.Credentials;
namespace DnkGallery.Model.Github;

public interface IGitApi {
    public Task<IReadOnlyList<RepositoryContent>?> GetReposContent(string repos, string? path = default);
    
    public Task<byte[]?> GetReposRawContent(string repos, string? path = default);
    
    public Task Clone(string repos, string localPath);
    
    public Task<RepositoryStatus?> Status(string localReposPath);
    
    public Task Fetch(string repos, string? remoteName = default, FetchOptions? options = default);
    
    public Task<MergeResult> Pull(string localReposPath, Identity identity);
    
    public Task PullRequest(string repos, string accessToken, string title, string branch);
    
    public bool CheckWorkDir(string localReposPath);
    
    public Task Push(string localReposPath, Credentials credentials, string? remoteName = "origin");
    
    public Task Commit(string localReposPath, string message, Identity identity);
    
    public Task Add(string localReposPath, IEnumerable<string> file);
}
