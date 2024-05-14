namespace DnkGallery.Model;

public class GalleryService : IGalleryService{
    public async Task<IList<Chapter>> Chapters(string dir) {
        var directoryInfo = new DirectoryInfo(dir);
        var chapters = await Task.Run(()=> TreeChapter(directoryInfo));
        return chapters;
    }
    
    private List<Chapter> TreeChapter(DirectoryInfo directoryInfo) {
        var directoryInfos = directoryInfo.GetDirectories("*",SearchOption.TopDirectoryOnly);
        return directoryInfos.Select(x => new Chapter(x.Name, x.FullName, x.GetDirectories().Length > 0)).ToList();
    }

}
