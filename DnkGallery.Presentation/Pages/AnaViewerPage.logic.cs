using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using DnkGallery.Model;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
namespace DnkGallery.Presentation.Pages;
using KeyboardAccelerator = Microsoft.UI.Xaml.Input.KeyboardAccelerator;
[UIBindable]
public sealed partial class AnaViewerPage : BasePage<BindableAnaViewViewModel>, IBuildUI {
    private UIControls.ScrollViewer scrollViewer;
    private UIControls.HyperlinkButton zoomResetButton;
    private UIControls.AppBarButton zoomInButton;
    private UIControls.AppBarButton zoomOutButton;
    private UIControls.AppBarButton copyButton;
    public AnaViewerPage() => BuildUI();
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        var ana = e.Parameter as NavigationParameter<Ana>;
        await vm.Model.Ana.Update(_ => ana.Payload, CancellationToken.None);
        base.OnNavigatedTo(e);
    }
    private void ImageInvoke(UIControls.Image image) {
        image.DragStarting += (sender, args) => {
            // new Vector2(args.GetPosition());
        };
    }
    private void ScrollViewerInvoke(UIControls.ScrollViewer obj) {
        obj.ZoomToFactor(1.0F);
    }
    private void ContentInvoke(UIControls.Page obj) {
        zoomResetButton.Tapped +=  (sender, args) => scrollViewer.ZoomToFactor(1.0F);
        copyButton.Tapped += async (sender, args) => {
             await Copy();
        };
        // obj.Loaded += (sender, args) => {
            RegisterAccelerator();
        // };
    }
    /// <summary>
    /// Ctrl + C复制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void CopyInvoke(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args) {
        await Copy();
    }
    
    public async Task Copy() {
        var ana = await vm.Model.Ana;
        if(ana is null)
            return;
        var randomAccessStream =
            await FileRandomAccessStream.OpenAsync(ana.Path, FileAccessMode.Read);
        
        Clipboarder.CopyImage(randomAccessStream);
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
        scrollViewer.KeyboardAccelerators.Add(copyAccelerator);
    }
}

public partial record AnaViewViewModel : BaseViewModel {
    public IState<Ana> Ana => UseState(() => new Ana(default, default, default));
    
    public IState<float> Zoom => UseState(() => 1.0F);
    
    public async Task ZoomIn() {
        await SetState(Zoom, zoom => zoom + 0.1F);
    }
    public async Task ZoomOut() {
        await SetState(Zoom, zoom => zoom - 0.1F);
    }
    

}