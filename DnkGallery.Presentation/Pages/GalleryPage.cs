using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = CSharpMarkup.WinUI.DataTemplate;

namespace DnkGallery.Presentation.Pages;

public partial class GalleryPage {
    public void BuildUI() => Content(
        Grid(
            Columns(Star, Auto),
            ItemsView()
                
                // .ItemsSource().Bind(vm?.Anas)
                .ItemTemplate().BindTemplate(ItemViewTemplate)
                .Assign(out itemsView)
                .Invoke(ItemsViewInvoke)
                .Layout(new UIControls.LinedFlowLayout {
                    ItemsStretch = UIControls.LinedFlowLayoutItemsStretch.Fill,
                    LineHeight = 160,
                    LineSpacing = 5,
                    MinItemSpacing = 5
                }),
            AnnotatedScrollBar()
                .Assign(out scrollBar)
                .Margin(4, 0, 48, 0).HorizontalAlignment(HorizontalAlignment.Right)
        ).Grid_Column(1)
    );
    
    private DataTemplate ItemViewTemplate => DataTemplate(() => 
        ItemContainer(
            Grid(
                    Image().Source().Bind("ImageSource")
                        .Stretch(Stretch.UniformToFill).HCenter().VCenter().MinWidth(100),
                    VStack(
                            TextBlock().Text().Bind("Title")
                            ).Height(32).VerticalAlignment(VerticalAlignment.Bottom).Padding(5,1,5,1)
                        .Background("SystemControlBackgroundBaseMediumBrush").Opacity(0.75)
                
                )
            ).AutomationProperties_Name("Name")
        );
}
