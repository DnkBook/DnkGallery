using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = CSharpMarkup.WinUI.DataTemplate;
namespace DnkGallery.Presentation.Pages;

public partial class GitPage {
    public static readonly string Header = "Git";
    public void BuildUI() => Content(
        Grid(
            Rows(Star, Auto),
            VStack(
                TextBlock("变更列表")
                    .FontWeight(FontWeights.Bold).FontSize(24),
                GridView()
                    .ItemsSource().Bind(vm?.AddedAnas)
                    .ItemTemplate(GridViewTemplate)
                    .Assign(out gridView)).Spacing(12),
            Grid(
                Columns(Star, Auto),
               TextBox().Header("提交信息")
                   .AcceptsReturn(true)
                   .TextWrapping(TextWrapping.Wrap)
                   .MaxHeight(200)
                   .ScrollViewer_VerticalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
                   .Text().Bind(vm?.Message, BindingMode.TwoWay),
                   
                
                   Button("提交")
                       .Style(ThemeResource.AccentButtonStyle)
                       .BindCommand(vm?.Commit)
                       .Grid_Column(1)
                       .VerticalAlignment(VerticalAlignment.Bottom)
                       .HCenter().Margin(16,0)
               ).Grid_Row(1)
        ).Margin(24)
    ).Invoke(ContentInvoke);
    
    private DataTemplate GridViewTemplate => DataTemplate(() =>
        Grid(
            Image().Source().Bind("Path")
                .Stretch(Stretch.UniformToFill).HCenter().VCenter(),
            VStack(
                    TextBlock()
                        .Text()
                        .Bind("Name")
                        .Padding(4)
                        .HCenter().VCenter().FontSize(16)
                )
                .VerticalAlignment(VerticalAlignment.Bottom)
                .Background(ThemeResource.SolidBackgroundFillColorBaseAltBrush)
                .Opacity(0.75)
        ).Width(300).Height(200)
    );
}
