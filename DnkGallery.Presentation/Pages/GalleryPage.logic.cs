using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Uno.Extensions;
using BitmapImage = Microsoft.UI.Xaml.Media.Imaging.BitmapImage;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
using Path = System.IO.Path;
using WriteableBitmap = Microsoft.UI.Xaml.Media.Imaging.WriteableBitmap;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GalleryPage : BasePage<BindableGalleryViewModel>, IBuildUI {
    private UIControls.GridView itemsView;
    private UIControls.ContentDialog saveDialog;
    
    public GalleryPage() {
        BuildUI();
    }
    
    private void ContentInvoke(UIControls.Page obj) {
        obj.Loaded += (sender, args) => {
            itemsView.ItemTemplate = ItemViewTemplate;
            RegisterAccelerator();
        };
    }
    
    private void RegisterAccelerator() {
        var copyAccelerator = new KeyboardAccelerator {
            Key = VirtualKey.C,
            Modifiers = VirtualKeyModifiers.Control
        };
        copyAccelerator.Invoked += CopyInvoke;
        itemsView.KeyboardAccelerators.Add(copyAccelerator);
        
        var pasteAccelerator = new KeyboardAccelerator {
            Key = VirtualKey.V,
            Modifiers = VirtualKeyModifiers.Control
        };
        pasteAccelerator.Invoked += PasteInvoke;
        itemsView.KeyboardAccelerators
            .Add(pasteAccelerator);
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        var chapter = e.Parameter as Chapter;
        await vm.Model.Chapter.Update(_ => chapter, CancellationToken.None);
        vm.Model.LoadAnas();
        
        base.OnNavigatedTo(e);
    }
    
    private async void CopyInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        if (itemsView.SelectedItem is not Ana itemsViewSelectedItem)
            return;
        var dataPackage = new DataPackage();
        var randomAccessStream =
            await FileRandomAccessStream.OpenAsync(itemsViewSelectedItem.Path, FileAccessMode.Read);
        var randomAccessStreamReference = RandomAccessStreamReference.CreateFromStream(randomAccessStream);
        dataPackage.SetBitmap(randomAccessStreamReference);
        Clipboard.SetContent(dataPackage);
    }
    
    private async void PasteInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        var dataPackageView = Clipboard.GetContent();
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap)) {
            return;
        }
        
        var randomAccessStreamReference = await dataPackageView.GetBitmapAsync();
        var randomAccessStreamWithContentType = await randomAccessStreamReference.OpenReadAsync();
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(randomAccessStreamWithContentType);
        randomAccessStreamWithContentType.Seek(0);
        var writeableBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
        await writeableBitmap.SetSourceAsync(randomAccessStreamWithContentType);
        var stream = writeableBitmap.PixelBuffer.AsStream();
        
        var saveAnaData = new SaveAnaData() {
            Stream = stream,
            FileName = Ana.NewFileName,
            Image = bitmapImage,
            PixelWidth = bitmapImage.PixelWidth,
            PixelHeight = bitmapImage.PixelHeight
        };
        vm.Model.SaveData.Update(_ => saveAnaData, CancellationToken.None);
        await saveDialog.ShowAsync();
    }
}

public record SaveAnaData {
    public Stream Stream { get; init; }
    public int PixelWidth { get; init; }
    public int PixelHeight { get; init; }
    public BitmapImage Image { get; init; }
    public string FileName { get; init; } = Ana.NewFileName;
}

public partial record GalleryViewModel : BaseViewModel {
    
    public IState<ObservableCollection<Ana>> Anas => UseState(() => new ObservableCollection<Ana>());
    public IState<Chapter> Chapter => UseState(() => new Chapter(default, default, default));
    
    public IState<SaveAnaData> SaveData => UseState(() => new SaveAnaData());
    
    
    public async Task Save() {
        var chapter = await Chapter;
        var saveAnaData = await SaveData;
        var saveFileName = saveAnaData.FileName;
        await using var pixelStream = saveAnaData.Stream;
        
        var path = Path.Combine(chapter.Dir, saveFileName);
        var folderFromPath = await StorageFolder.GetFolderFromPathAsync(chapter.Dir);
        var storageFile = await folderFromPath.CreateFileAsync(saveFileName);
        using var randomAccessStream =
            await storageFile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.AllowReadersAndWriters);
        
        var bitmapEncodeGuid = GetBitmapEncodeGuid(Path.GetExtension(saveFileName));
        var bitmapEncoder = await BitmapEncoder.CreateAsync(bitmapEncodeGuid, randomAccessStream);
        
        
        var pixels = new byte[pixelStream.Length];
        await pixelStream.ReadAsync(pixels);
        bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
            (uint)saveAnaData.PixelWidth,
            (uint)saveAnaData.PixelHeight,
            96.0,
            96.0,
            pixels);
        await bitmapEncoder.FlushAsync();
        
        await LoadAnas();
    }
    
    public async Task LoadAnas() {
        var galleryService = Service.GetService<IGalleryService>()!;
        var anas = await galleryService.Anas(await Chapter);
        await Anas.Update(_ => anas.ToObservableCollection(), CancellationToken.None);
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
