using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = Microsoft.UI.Xaml.DataTemplate;
using ImageSource = Microsoft.UI.Xaml.Media.ImageSource;
namespace DnkGallery.Presentation.Pages;

public partial class GalleryPage {
    public void BuildUI() => Content(Grid(Columns(Star, Auto),
        GridView()
            .Assign(out itemsView),
        AnnotatedScrollBar()
            .Assign(out scrollBar)
            .Margin(4, 0, 48, 0).HorizontalAlignment(HorizontalAlignment.Right).Grid_Column(1))).Invoke(ContentInvoke);
    
    private DataTemplate ItemViewTemplate => DataTemplate(() => 
        Grid(
            Image().Source().Bind("ImageSource")
            .Stretch(Stretch.UniformToFill).HCenter().VCenter().MinWidth(100),
        VStack(
                TextBlock().Text().Bind("Name")
                )
            .Height(32)
            .VerticalAlignment(VerticalAlignment.Bottom)
            .HCenter().Width(300)
            .Background(ThemeResource.SystemRevealBaseMediumColor)
            .Opacity(0.75)
        ).Width(300).Height(200)
        );
}
