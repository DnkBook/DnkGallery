namespace DnkGallery.Model;

public record Ana(string Path, DateTime DateTime, string Name) {
    
    public Lazy<byte[]?> ImageBytes { get; set; }
    public static string NewFileName => DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".jpg";
}
