using Windows.Graphics.Imaging;
using Windows.Storage;
using DnkGallery.Presentation.Pages;
using Path = System.IO.Path;
namespace DnkGallery.Presentation.Utils;

public static class Storage {
    public static async Task SaveImage(StorageSaveImageData storageSaveImageData) {
        var imageBytes = storageSaveImageData.ImageBytes;
        
        var directoryName = Path.GetDirectoryName(storageSaveImageData.FullName);
        
        var folderFromPath = await StorageFolder.GetFolderFromPathAsync(directoryName);
        var storageFile = await folderFromPath.CreateFileAsync(storageSaveImageData.FileName);
        using var randomAccessStream =
            await storageFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowReadersAndWriters);
        
        var bitmapEncodeGuid = GetBitmapEncodeGuid(Path.GetExtension(storageSaveImageData.FileName));
        var bitmapEncoder = await BitmapEncoder.CreateAsync(bitmapEncodeGuid, randomAccessStream);
        
        bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
            (uint)storageSaveImageData.PixelWidth,
            (uint)storageSaveImageData.PixelHeight,
            96.0,
            96.0,
            imageBytes);
        await bitmapEncoder.FlushAsync();
    }
    
    private static Guid GetBitmapEncodeGuid(string filename) => filename switch {
        ".jpg" => BitmapEncoder.JpegEncoderId,
        ".png" => BitmapEncoder.PngEncoderId,
        ".bmp" => BitmapEncoder.BmpEncoderId,
        ".tiff" => BitmapEncoder.TiffEncoderId,
        ".gif" => BitmapEncoder.GifEncoderId,
        _ => BitmapEncoder.JpegEncoderId,
    };
}

public class StorageSaveImageData(string filename) {
    public byte[]? ImageBytes { get; init; }
    public int PixelWidth { get; init; }
    public int PixelHeight { get; init; }
    public string FullName { get; init; }
    public UI.Xaml.Media.Imaging.BitmapImage? Image { get; init; }
    public string FileName { get; init; } = filename;
}
