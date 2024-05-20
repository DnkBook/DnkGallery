using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
namespace DnkGallery.Presentation.Utils;

public static class Clipboarder {
    public static void CopyImage(IRandomAccessStream randomAccessStream) {
        var dataPackage = new DataPackage();
        var randomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(randomAccessStream);
        dataPackage.SetBitmap(randomAccessStreamReference);
        Clipboard.SetContent(dataPackage);
    }
    
    public static async Task CopyImage(byte[] imageBytes) {
        var randomAccessStream = new InMemoryRandomAccessStream();
        var stream = randomAccessStream.AsStream();
        await stream.WriteAsync(imageBytes);
        await stream.FlushAsync();
        CopyImage(randomAccessStream);
    }
    
    public static async Task<(UI.Xaml.Media.Imaging.BitmapImage? image, byte[]?)> PasteImage() {
        var dataPackageView = Clipboard.GetContent();
        //判断是否是图片
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
            return default;
        
        var randomAccessStreamReference = await dataPackageView.GetBitmapAsync();
        using var randomAccessStreamWithContentType = await randomAccessStreamReference.OpenReadAsync();
        
        // 这里保存到BitmapImage用于显示
        var bitmapImage = new UI.Xaml.Media.Imaging.BitmapImage();
        await bitmapImage.SetSourceAsync(randomAccessStreamWithContentType);
        // 设置到初始位置再读一遍
        randomAccessStreamWithContentType.Seek(0);
        // 保存到WriteableBitmap获得流后面用于保存
        var writeableBitmap = new UI.Xaml.Media.Imaging.WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
        await writeableBitmap.SetSourceAsync(randomAccessStreamWithContentType);
        var writeableBitmapPixelBuffer = writeableBitmap.PixelBuffer;
        using var dataReader = DataReader.FromBuffer(writeableBitmapPixelBuffer);
        var bytes = new byte[writeableBitmapPixelBuffer.Length];
        dataReader.ReadBytes(bytes);
        
        return (bitmapImage, bytes);
    }
}
