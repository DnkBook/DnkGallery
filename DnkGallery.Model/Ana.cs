namespace DnkGallery.Model;

public record Ana(string Path, DateTime DateTime, string Name) {
    public static string NewFileName => DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".jpg";
}
