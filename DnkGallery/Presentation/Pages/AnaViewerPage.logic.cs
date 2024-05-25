using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using DnkGallery.Model;
namespace DnkGallery.Presentation.Pages;
[UIBindable]
public sealed partial class AnaViewerPage : BasePage<BindableAnaViewViewModel>, IBuildUI {
    private UIControls.ScrollViewer scrollViewer;
    private UIControls.HyperlinkButton zoomResetButton;
    private UIControls.AppBarButton zoomInButton;
    private UIControls.AppBarButton zoomOutButton;
    private UIControls.AppBarButton copyButton;
    private UIControls.AppBarButton prevButton;
    private UIControls.AppBarButton nextButton;
    public AnaViewerPage() => BuildUI();
    protected override async void OnNavigatedTo(UIXaml.Navigation.NavigationEventArgs e) {
        var parameter = e.Parameter as NavigationParameter<(IImmutableList<Ana> Anas, Ana Ana, int Index)>;
        await vm.Model.Anas.Update(_ => parameter.Payload.Anas, CancellationToken.None);
        await vm.Model.Index.Update(_ => parameter.Payload.Index, CancellationToken.None);
        await vm.Model.Ana.Update(_ => parameter.Payload.Ana, CancellationToken.None);
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
        zoomResetButton.Tapped += (sender, args) => scrollViewer.ZoomToFactor(1.0F);
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
    private async void CopyInvoke(UIXaml.Input.KeyboardAccelerator sender, UIXaml.Input.KeyboardAcceleratorInvokedEventArgs args) {
        await Copy();
    }
    
    public async Task Copy() {
        var ana = await vm.Model.Ana;
        if (ana is null)
            return;
        var randomAccessStream =
            await FileRandomAccessStream.OpenAsync(ana.Path, FileAccessMode.Read);
        
        Clipboarder.CopyImage(randomAccessStream);
    }
    
    /// <summary>
    /// 订阅快捷键事件
    /// </summary>
    private void RegisterAccelerator() {
        var copyAccelerator = new UIXaml.Input.KeyboardAccelerator {
            Key = VirtualKey.C,
            Modifiers = VirtualKeyModifiers.Control
        };
        copyAccelerator.Invoked += CopyInvoke;
        scrollViewer.KeyboardAccelerators.Add(copyAccelerator);
    }
}

public partial record AnaViewViewModel : BaseViewModel {
    
    public IState<Ana> Ana => State<Ana>.Empty(this);
    public IListState<Ana> Anas => ListState<Ana>.Empty(this);

    public IState<int> Index => UseState(() => 0);

    public IState<float> Zoom => UseState(() => 1.0F);
    
    public async Task ZoomIn() {
        await SetState(Zoom, zoom => zoom + 0.1F);
    }
    public async Task ZoomOut() {
        await SetState(Zoom, zoom => zoom - 0.1F);
    }
    public async Task Prev() {
        var anas = await Anas;
        await SetState(Index, index => index <= 0 ? anas?.Count - 1 ?? 0 : index - 1);
        var index = await Index;
        await SetState(Ana, _ => anas?[index]);
    }
    public async Task Next() {
        var anas = await Anas;
        await SetState(Index, index => index >= anas?.Count - 1 ? 0 : index + 1);
        var index = await Index;
        await SetState(Ana, _ => anas?[index]);
    }
    
}
