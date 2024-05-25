namespace DnkGallery.Model.Services;

public class LocalGalleryService: IGalleryService {
    public async Task<IList<Chapter>> Chapters(string dir) {
        var directoryInfo = new DirectoryInfo(dir);
        if (!directoryInfo.Exists) {
            directoryInfo.Create();
        }
        var chapters = await Task.Run(() => TreeChapter(directoryInfo));
        return chapters;
    }
    public async IAsyncEnumerable<Ana> Anas(Chapter chapter) {
        var directoryInfo = new DirectoryInfo(chapter.Dir);
        var list = Task.Run(() => directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
            .Where(x => Ana.NameFilter(x.FullName))
            .Select(x => new Ana(x.Name,x.FullName)).ToList());
        foreach (var ana in await list) {
            var anaPath = ana.Path;
            var readAllBytes = await File.ReadAllBytesAsync(anaPath);
            ana.ImageBytes = readAllBytes;
            ana.LocalExists = true;
            yield return ana;
        }
    }
    
    private List<Chapter> TreeChapter(DirectoryInfo directoryInfo) {
        var directoryInfos = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
        return directoryInfos.Where(x => !x.Name.StartsWith('.')).Select(x =>
            new Chapter(x.Name, x.FullName, x.GetDirectories().Length > 0, [x.Name])).ToList();
    }
}
