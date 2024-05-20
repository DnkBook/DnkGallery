namespace DnkGallery.Model.Services;

public interface IGalleryService {
    Task<IList<Chapter>> Chapters(string dir);
    IAsyncEnumerable<Ana> Anas(Chapter chapter);
}
