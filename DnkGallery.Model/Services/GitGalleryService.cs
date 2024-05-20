using System.Security.AccessControl;
using DnkGallery.Model.Github;
using Octokit;
using FileMode = System.IO.FileMode;

namespace DnkGallery.Model.Services;

public class GitGalleryService(IGitApi gitApi, Setting setting)
    : IGalleryService {
    public async Task<IList<Chapter>> Chapters(string dir) {
        var settingGitRepos = setting.GitRepos;
        var contentDirs = await ContentDirs(settingGitRepos, dir);
        var tasks = contentDirs.Select(async x => {
            var reposChildrenContent = await gitApi.GetReposContent(settingGitRepos, x.Path);
            var count = reposChildrenContent?.Where(x => x.Type == ContentType.Dir).Count();
            var chapter = new Chapter(x.Name, x.Path, count > 0, [x.Name]);
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
        
        var reposContent = await gitApi.GetReposContent(settingGitRepos, chapter.Dir);
        var anas = reposContent?
            .Where(x => Ana.NameFilter(x.Name)) ?? [];
        foreach (var content in anas) {
            var ana = await GetLocalIfNotExistsSaveIt(content.Path, async () => {
                var ana = new Ana(content.Name, content.Path);
                var reposRawContent = await gitApi.GetReposRawContent(settingGitRepos, ana.Path);
                ana.ImageBytes = reposRawContent;
                return ana;
            });
            
            yield return ana;
        }
    }
    
    /// <summary>
    /// 保存在本地
    /// </summary>
    /// <param name="path"></param>
    /// <param name="saveProvider"></param>
    /// <returns></returns>
    private async Task<Ana> GetLocalIfNotExistsSaveIt(string path, Func<Task<Ana>>? saveProvider) {
        var settingLocalPath = setting.LocalPath;
        var fullName = Path.Combine(settingLocalPath, path);
        if (File.Exists(fullName)) {
            var fileName = Path.GetFileName(fullName);
            var ana = new Ana(fileName, fullName);
            var readAllBytes = await File.ReadAllBytesAsync(fullName);
            ana.ImageBytes = readAllBytes;
            return ana;
        }
        // TODO SAVE ERROR
        var saveAna = await saveProvider?.Invoke();
        await File.WriteAllBytesAsync(fullName, saveAna.ImageBytes);
        return saveAna;
    }
    
    //不用递归 限制很大
    // private async Task TreeAnas(string settingSourcePath, List<Ana> anas, string dir) {
    //     var reposContent = await gitApi.GetReposContent(settingSourcePath, dir);
    //     
    //     var gitReposContents = reposContent?
    //         .Where(x => x.Name.ToLower().EndsWith(".jpg") || x.Name.ToLower().EndsWith(".png") ||
    //                     x.Name.ToLower().EndsWith(".jpeg")) ?? [];
    //     foreach (var gitReposContent in gitReposContents) {
    //         var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(gitReposContent.Name);
    //         var dateTime = DateTime.ParseExact(fileNameWithoutExtension, "yyyy-MM-dd_HHmmss", null);
    //         var ana = new Ana(gitReposContent.DownloadUrl, dateTime, gitReposContent.Name);
    //         anas.Add(ana);
    //         
    //         await TreeAnas(settingSourcePath, anas, gitReposContent.Path);
    //     }
    // }
}