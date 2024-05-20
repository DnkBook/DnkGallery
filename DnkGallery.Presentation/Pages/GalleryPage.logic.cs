using System.Collections.Immutable;
using Windows.System;
using DnkGallery.Model;
using DnkGallery.Model.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
using Path = System.IO.Path;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GalleryPage : BasePage<BindableGalleryViewModel>, IBuildUI {
    private UIControls.GridView gridView;
    private UIControls.ContentDialog saveDialog;
    public GalleryPage() {
        BuildUI();
    }
    
    private void ContentInvoke(UIControls.Page obj) {
        // obj.Loaded += (sender, args) => {
        // itemsView.ItemTemplate = ItemViewTemplate;
        RegisterAccelerator();
        // };
    }
    
    private void GridViewItemInvoke(UIControls.Grid obj) {
        obj.DoubleTapped += async (sender, args) => {
            if (obj.DataContext is not Ana ana)
                return;
            
            var immutableList = await vm.Model.Anas.Value(CancellationToken.None);
            
            Navigater.Navigate(ana.Path, typeof(AnaViewerPage), ana.Name,
                new NavigationParameter<(IImmutableList<Ana> Anas, Ana ana, int SelectedIndex)>(ana.Path, [], (immutableList, ana, gridView.SelectedIndex)));
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
        gridView.KeyboardAccelerators.Add(copyAccelerator);
        
        var pasteAccelerator = new KeyboardAccelerator {
            Key = VirtualKey.V,
            Modifiers = VirtualKeyModifiers.Control
        };
        pasteAccelerator.Invoked += PasteInvoke;
        gridView.KeyboardAccelerators
            .Add(pasteAccelerator);
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        var parameter = e.Parameter as NavigationParameter<Chapter>;
        await vm.Model.Chapter.Update(_ => parameter.Payload, CancellationToken.None);
        await vm.Model.LoadAnas();
        base.OnNavigatedTo(e);
    }
    /// <summary>
    /// Ctrl + C复制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void CopyInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        if (gridView.SelectedItem is not Ana itemsViewSelectedItem)
            return;
        
        if (itemsViewSelectedItem.ImageBytes != null) {
            await Clipboarder.CopyImage(itemsViewSelectedItem.ImageBytes);
        }
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
        
        var (image, bytes) = await Clipboarder.PasteImage();
        var chapter = await vm.Model.Chapter;
        
        if (image is null || bytes is null || chapter is null)
            return;
        
        var newFileName = Ana.NewFileName;
        var saveAnaData = new StorageSaveImageData(newFileName) {
            ImageBytes = bytes,
            Image = image,
            FullName = Path.Combine(chapter.Dir, newFileName),
            PixelWidth = image.PixelWidth,
            PixelHeight = image.PixelHeight
        };
        await vm.Model.SaveData.Update(_ => saveAnaData, CancellationToken.None);
        await saveDialog.ShowAsync();
        
    }
}

public partial record GalleryViewModel : BaseViewModel {
    
    public IListState<Ana> Anas => ListState<Ana>.Empty(this);
    public IState<Chapter> Chapter => State<Chapter>.Empty(this);
    
    public IState<StorageSaveImageData> SaveData => State<StorageSaveImageData>.Empty(this);
    
    
    public async Task Save() {
        var saveAnaData = await SaveData;
        if (saveAnaData != null) {
            await Storage.SaveImage(saveAnaData);
            // 保存完reload一下
            await LoadAnas();
        }
    }
    
    public async Task LoadAnas() {
        var chapter = await Chapter;
        await Anas.RemoveAllAsync(_ => true, CancellationToken.None);
        var galleryService = Service.GetKeyedService<IGalleryService>(Settings.Source)!;
        var anas = galleryService.Anas(chapter);
        await foreach (var ana in anas) {
            // 这里保存很奇怪 还是用Git拉取吧
            // if (!ana.LocalExists) {
            //     var storageSaveImageData = new StorageSaveImageData(ana.Name) {
            //         ImageBytes = ana.ImageBytes,
            //         PixelWidth = 500,
            //         PixelHeight = 500,
            //         FullName = Path.Combine(Settings.LocalPath, ana.Path),
            //     };
            //     await Storage.SaveImage(storageSaveImageData);
            // }
            await Anas.AddAsync(ana);
        }
    }
    
    
}
