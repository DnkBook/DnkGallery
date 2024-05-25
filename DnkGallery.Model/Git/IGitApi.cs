using LibGit2Sharp;
using Octokit;
using Branch = LibGit2Sharp.Branch;

namespace DnkGallery.Model.Github;

public interface IGitApi {
    public Task<IReadOnlyList<RepositoryContent>?> GetReposContent(string repos, string? path = default);
    
    public Task<byte[]?> GetReposRawContent(string repos, string? path = default);
    
    public Task Clone(string repos, string localPath);
    
    public Task<RepositoryStatus?> Status(string localReposPath);
    
    public Task Fetch(string repos, string? remoteName = "origin");
    
    public Task<MergeResult> Pull(string localReposPath, string userName, string? email = default);
    
    public Task PullRequest(string repos,string localReposPath , string accessToken, string title, string branch ,string? baseBranch = "main");
    
    public bool CheckWorkDir(string localReposPath);
    
    public Task Push(string localReposPath, string userName, string accessToken, string? remoteName = "origin");
    
    public Task Commit(string localReposPath, string message, string userName, string? email = default);
    
    public Task Add(string localReposPath, IEnumerable<string> file);
    
    public Task<Branch> Checkout(string localReposPath, Branch branch);
    
    public Task<ICommitLog> BeingPushedCommits(string localReposPath);
    
    public Task<Branch> Branch(string localReposPath);
    
    public Task<IQueryableCommitLog> Commits(string localReposPath);
    
    Task<BranchCollection> Branches(string localReposPath);
    
    
    Task<Branch> CreateBranch(string localReposPath, string? branchName, string? remoteName = "origin");
}
