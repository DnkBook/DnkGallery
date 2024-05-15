namespace DnkGallery.Model;

public class GalleryService : IGalleryService {
    public async Task<IList<Chapter>> Chapters(string dir) {
        var directoryInfo = new DirectoryInfo(dir);
        var chapters = await Task.Run(() => TreeChapter(directoryInfo));
        return chapters;
    }
    public async Task<IList<Ana>> Anas(Chapter chapter) {
        var directoryInfo = new DirectoryInfo(chapter.Dir);
        var chapters = await Task.Run(() => {
            var list = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(x => x.FullName.ToLower().EndsWith(".jpg") || x.FullName.ToLower().EndsWith(".png") || x.FullName.ToLower().EndsWith(".jpeg"))
                .Select(x => DateTime.TryParse(x.Name, out var dateTime)
                    ? new Ana(x.FullName, dateTime, x.Name)
                    : new Ana(x.FullName, x.CreationTime, x.Name)).ToList();
            return list;
        });
        return chapters;
    }
    
    private List<Chapter> TreeChapter(DirectoryInfo directoryInfo) {
        var directoryInfos = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);
        return directoryInfos.Select(x => new Chapter(x.Name, x.FullName, x.GetDirectories().Length > 0)).ToList();
    }
    
}
