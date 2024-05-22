using LibGit2Sharp;
using Octokit;
namespace DnkGallery.Model.Github;

public interface IGitApi {
    public Task<IReadOnlyList<RepositoryContent>?> GetReposContent(string repos, string? path = default);
    
    public Task<byte[]?> GetReposRawContent(string repos, string? path = default);
    
    public Task Clone(string repos,string localPath);
    
    public Task<RepositoryStatus> Status(string localReposPath);
    
    public Task Fetch(string repos, string? remoteName = default, FetchOptions? options = default);
    
    public Task<MergeResult> Pull(string localReposPath, Identity identity);
    
}
