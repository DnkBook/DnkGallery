﻿using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
            // itemsView.ItemTemplate = ItemViewTemplate;
            RegisterAccelerator();
        };
    }
    /// <summary>
    /// 订阅快捷键事件
    /// </summary>
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
        await vm.Model.LoadAnas();
        base.OnNavigatedTo(e);
    }
    /// <summary>
    /// Ctrl + C复制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
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
    /// <summary>
    /// Ctrl + V粘贴，Win + V可选调出粘贴板
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void PasteInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        // 标记此事件已被处理
        // 有时事件会触发两次 dialog打开两次导致异常，不明所以
        args.Handled = true;
        // 判断当前有无dialog打开
        if (VisualTreeHelper.GetOpenPopups(MainWindow).Count > 0)
            return;
        
        var dataPackageView = Clipboard.GetContent();
        //判断是否是图片
        if (!dataPackageView.Contains(StandardDataFormats.Bitmap))
            return;
        
        var randomAccessStreamReference = await dataPackageView.GetBitmapAsync();
        using var randomAccessStreamWithContentType = await randomAccessStreamReference.OpenReadAsync();
        
        // 这里保存到BitmapImage用于显示
        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(randomAccessStreamWithContentType);
        // 设置到初始位置再读一遍
        randomAccessStreamWithContentType.Seek(0);
        // 保存到WriteableBitmap获得流后面用于保存
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
        await vm.Model.SaveData.Update(_ => saveAnaData, CancellationToken.None);
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
        
        // 保存完reload一下
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
