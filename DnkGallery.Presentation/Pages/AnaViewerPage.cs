namespace DnkGallery.Presentation.Pages;

public partial class AnaViewerPage {
    
    public void BuildUI() => Content(
        ScrollView(
            Image().HCenter().VCenter().Source().Bind(vm?.Ana?.Path).Width(500).Height(500)
            ).ScrollViewer_ZoomMode(UIControls.ZoomMode.Enabled)
            .IsTabStop(true)
            .ScrollViewer_HorizontalScrollMode(UIControls.ScrollMode.Auto)
            .ScrollViewer_VerticalScrollMode(UIControls.ScrollMode.Auto)
            .ScrollViewer_HorizontalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
            .ScrollViewer_VerticalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
        );
}
