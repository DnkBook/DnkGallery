using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GalleryPage : BasePage<GalleryViewModel>, IBuildUI {
    private UIControls.GridView itemsView;
    private UIControls.AnnotatedScrollBar? scrollBar;
    
    // private UIControls.AnnotatedScrollBar? scrollBar;
    private Chapter chapter;
    
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
        chapter = e.Parameter as Chapter;
        var galleryService = Service.GetService<IGalleryService>()!;
        var anas = await galleryService.Anas(chapter);
        // await vm.Anas.Update(_ => anas.ToObservableCollection(), CancellationToken.None);
        itemsView.ItemsSource = anas;
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
        
        var bitmapDecoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(randomAccessStreamWithContentType);
        var pixelDataProvider = await bitmapDecoder.GetPixelDataAsync(
            Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
            Windows.Graphics.Imaging.BitmapAlphaMode.Straight,
            new Windows.Graphics.Imaging.BitmapTransform(),
            Windows.Graphics.Imaging.ExifOrientationMode.RespectExifOrientation,
            Windows.Graphics.Imaging.ColorManagementMode.DoNotColorManage);
        var detachPixelData = pixelDataProvider.DetachPixelData();
        
        var fileSavePicker = new FileSavePicker {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            SuggestedFileName = DateTime.Now.ToString("yyyy-MM-dd_HHmmss")
        };
        fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        fileSavePicker.FileTypeChoices.Add("Image", new List<string> { ".jpg", ".png" });
        
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(fileSavePicker, hwnd);
        
        var saveFile = await fileSavePicker.PickSaveFileAsync();
        if (saveFile == null) {
            return;
        }
        
        CachedFileManager.DeferUpdates(saveFile);
        
        await FileIO.WriteBytesAsync(saveFile, detachPixelData);
        
        await CachedFileManager.CompleteUpdatesAsync(saveFile);
    }
}

public partial record GalleryViewModel : BaseViewModel {
    // public IState<ObservableCollection<Ana>> Anas => UseState(()=> new ObservableCollection<Ana>());
}
