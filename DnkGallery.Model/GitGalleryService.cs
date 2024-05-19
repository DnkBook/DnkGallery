using DnkGallery.Model.Github;
using Octokit;
namespace DnkGallery.Model;

public class GitGalleryService(IGitApi gitApi, Setting setting) : IGalleryService {
    public async Task<IList<Chapter>> Chapters(string dir) {
        var settingGitRepos = setting.GitRepos;
        var contentDirs = await ContentDirs(settingGitRepos, dir);
        var tasks = contentDirs.Select(async x => {
            var reposChildrenContent = await gitApi.GetReposContent(settingGitRepos, x.Path);
            var chapter = new Chapter(x.Name, x.Path, reposChildrenContent?.Count > 0, [x.Name]);
            return chapter;
        }).ToList();
        var whenAll = await Task.WhenAll(tasks);
        return whenAll;
    }
    
    private async Task<IEnumerable<RepositoryContent>> ContentDirs(string repos, string dir) {
        var reposContent = await gitApi.GetReposContent(repos, dir);
        var dirs = reposContent?
            .Where(x => x.Type == ContentType.Dir);
        return dirs ?? [];
    }
    
    public async IAsyncEnumerable<Ana> Anas(Chapter chapter) {
        var settingGitRepos = setting.GitRepos;
        var anas = new List<Ana>();
        await TreeAnas(settingGitRepos, anas, chapter.Dir);
        
        foreach (var ana in anas) {
            var anaPath = ana.Path;
            var reposRawContent = await gitApi.GetReposRawContent(anaPath);
            // await Task.Delay(200);
            ana.ImageBytes = new Lazy<byte[]?>(() => reposRawContent);
            yield return ana;
        }
    }
    
    private async Task TreeAnas(string settingSourcePath, List<Ana> anas, string dir) {
        var reposContent = await gitApi.GetReposContent(settingSourcePath, dir);
        
        var gitReposContents = reposContent?
            .Where(x => x.Name.ToLower().EndsWith(".jpg") || x.Name.ToLower().EndsWith(".png") || x.Name.ToLower().EndsWith(".jpeg")) ?? [];
        foreach (var gitReposContent in gitReposContents) {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(gitReposContent.Name);
            var dateTime = DateTime.ParseExact(fileNameWithoutExtension,"yyyy-MM-dd_HHmmss", null);
            var ana = new Ana(gitReposContent.DownloadUrl, dateTime, gitReposContent.Name);
            anas.Add(ana);
            
            await TreeAnas(settingSourcePath,anas, gitReposContent.Path);
        }

    }
}
