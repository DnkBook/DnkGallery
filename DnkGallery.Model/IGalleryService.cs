namespace DnkGallery.Model;

public interface IGalleryService {
    Task<IList<Chapter>> Chapters(string dir);
    Task<IList<Ana>> Anas(Chapter chapter);
}
