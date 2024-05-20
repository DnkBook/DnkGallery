using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = Microsoft.UI.Xaml.DataTemplate;
namespace DnkGallery.Presentation.Pages;

public partial class AnaViewerPage {
    public void BuildUI() => Content(
        Grid(
            Rows(Star,Auto ,Auto),
            
        ScrollViewer(
                Image()
                    .HCenter()
                    .VCenter()
                    .Source().Bind(vm?.Ana?.ImageBytes, convert:  (byte[] bytes) =>  ByteArrayConvertToBitmapImage(bytes))
                .Invoke(ImageInvoke)
                ).ScrollViewer_ZoomMode(UIControls.ZoomMode.Enabled)
                .IsTabStop(true)
                .ScrollViewer_HorizontalScrollMode(UIControls.ScrollMode.Auto)
                .ScrollViewer_VerticalScrollMode(UIControls.ScrollMode.Auto)
                .ScrollViewer_HorizontalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
                .ZoomFactor().Bind(vm?.Zoom,BindingMode.TwoWay)
                .Invoke(ScrollViewerInvoke)
                .Assign(out scrollViewer),
        // GridView()
        //     .ScrollViewer_HorizontalScrollMode(UIControls.ScrollMode.Auto)
        //     .ScrollViewer_VerticalScrollMode(UIControls.ScrollMode.Disabled)
        //     .ScrollViewer_HorizontalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
        //     .ScrollViewer_VerticalScrollBarVisibility(UIControls.ScrollBarVisibility.Disabled)
        //     .Grid_Row(1)
        //     .ItemsSource().Bind(vm?.Anas)
        //     .SelectedIndex().Bind(vm?.Index,BindingMode.TwoWay)
        //     .SelectedItem().Bind(vm?.Ana,BindingMode.TwoWay)
        //     .ItemsPanel(GridViewPanelTemplate)
        //     .ItemTemplate(GridViewTemplate),
        HStack(
            HyperlinkButton()
                .Content()
                .Bind(vm?.Zoom, convert:(float zoom) => $"{zoom: #%}")
                .ToolTipService_ToolTip("还原")
                .Assign(out zoomResetButton),
            AppBarButton()
                .Icon(SymbolIcon(UIControls.Symbol.ZoomIn).UI)
                .BindCommand(vm?.ZoomIn)
                .ToolTipService_ToolTip("放大")
                .Assign(out zoomInButton),
            AppBarButton()
                .Icon(SymbolIcon(UIControls.Symbol.ZoomOut).UI)
                .BindCommand(vm?.ZoomOut)
                .ToolTipService_ToolTip("缩小")
                .Assign(out zoomOutButton),
            AppBarSeparator(),
            AppBarButton()
                .Icon(SymbolIcon(UIControls.Symbol.Back).UI)
                .ToolTipService_ToolTip("上一张")
                .BindCommand(vm?.Prev)
                .Assign(out prevButton),
            AppBarButton()
                .Icon(SymbolIcon(UIControls.Symbol.Forward).UI)
                .ToolTipService_ToolTip("下一张")
                .BindCommand(vm?.Next)
                .Assign(out nextButton),
            AppBarSeparator(),
            AppBarButton()
                .Icon(SymbolIcon(UIControls.Symbol.Copy).UI)
                .ToolTipService_ToolTip("复制")
                .Assign(out copyButton)
            )
            .Height(48).HCenter().VerticalAlignment(VerticalAlignment.Bottom)
        )
    ).Invoke(ContentInvoke);
    
    private DataTemplate GridViewTemplate => DataTemplate(() =>
        Grid(
            Image().Source().Bind("Path")
                .Stretch(Stretch.UniformToFill).HCenter().VCenter()
        ).Width(150).Height(100)
    );
    private static ItemsPanelTemplate GridViewPanelTemplate => 
        ItemsPanelTemplate(() => (CSharpMarkup.WinUI.UIElement)ItemsWrapGrid().Orientation(UIControls.Orientation.Horizontal));
    
}
