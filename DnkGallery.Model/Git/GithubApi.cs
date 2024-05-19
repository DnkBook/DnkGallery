using System.Text.Json;
using System.Text.Json.Serialization;
using Octokit;
namespace DnkGallery.Model.Github;

public class GithubApi(GitHubClient githubClient) : IGitApi {
    
    public static GithubApi Create() {
        var client = new GitHubClient(new ProductHeaderValue("DnkGallery"));
        return new GithubApi(client);
    }
    
    public async Task<IReadOnlyList<RepositoryContent>?> GetReposContent(string repos, string? path = "/") {
        var strings = repos.Split("/");
        var response = await githubClient.Repository.Content.GetAllContents(strings[0], strings[1],path);
        return response;
    }
    
    public async Task<byte[]?> GetReposRawContent(string repos, string? path = "/") {
        var strings = repos.Split("/");
        var rawContent = await githubClient.Repository.Content.GetRawContent(strings[0], strings[1],path);
        return rawContent;
    }
}
