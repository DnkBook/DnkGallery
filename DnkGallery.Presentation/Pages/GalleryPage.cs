using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace DnkGallery.Presentation.Pages;

public partial class GalleryPage {
    public void BuildUI() => Content(Grid(Columns(Star, Auto),
        GridView()
            .Assign(out itemsView)
        )
    ).Invoke(ContentInvoke);
        
    private DataTemplate ItemViewTemplate => DataTemplate(() => 
        Grid(
            Image().Source().Bind("Path")
            .Stretch(Stretch.UniformToFill).HCenter().VCenter().MinWidth(100),
        VStack(
                TextBlock()
                    .Text().Bind("Name").HCenter().VCenter().FontSize(16)
                )
            .Height(32)
            .VerticalAlignment(VerticalAlignment.Bottom)
            .Width(300)
            .Background(ThemeResource.AcrylicBackgroundFillColorDefaultBrush)
            .Opacity(0.75)
        ).Width(300).Height(200)
    );
}
