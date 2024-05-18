using Microsoft.UI.Xaml;
namespace DnkGallery.Presentation.Pages;

public partial class AnaViewerPage {
    public void BuildUI() => Content(
        Grid(
            Rows(Star, Auto),
            
        ScrollViewer(
                Image()
                    .HCenter()
                    .VCenter()
                    .Source().Bind(vm?.Ana?.Path)
                .Invoke(ImageInvoke)
                ).ScrollViewer_ZoomMode(UIControls.ZoomMode.Enabled)
                .IsTabStop(true)
                .ScrollViewer_HorizontalScrollMode(UIControls.ScrollMode.Auto)
                .ScrollViewer_VerticalScrollMode(UIControls.ScrollMode.Auto)
                .ScrollViewer_HorizontalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
                .ZoomFactor().Bind(vm?.Zoom,BindingMode.TwoWay)
                .Invoke(ScrollViewerInvoke)
                .Assign(out scrollViewer),
            
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
                .Icon(SymbolIcon(UIControls.Symbol.Copy).UI)
                .ToolTipService_ToolTip("复制")
                .Assign(out copyButton)
            )
            .Height(48).HCenter().VerticalAlignment(VerticalAlignment.Bottom)
        )
    ).Invoke(ContentInvoke);
}
