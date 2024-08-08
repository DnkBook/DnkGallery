namespace DnkGallery.Presentation.Pages;

public partial class GitPage {
    public static readonly string Header = "Git";
    public void BuildUI() => Content(
        Grid(
            Rows(Auto,Star, Auto),
            VStack(
            TextBlock("变更列表")
            .FontWeight(UI.Text.FontWeights.Bold).FontSize(24)).Spacing(12),
            GridView()
                .ItemsSource().Bind(vm?.AddedAnas)
                .ItemTemplate(GridViewTemplate)
                .Assign(out gridView).Grid_Row(1),
            Grid(
                Columns(Star, Auto),
               TextBox().Header("提交信息")
                   .AcceptsReturn(true)
                   .TextWrapping(UIXaml.TextWrapping.Wrap)
                   .MaxHeight(200)
                   .ScrollViewer_VerticalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
                   .Text().Bind(vm?.Message, BindingMode.TwoWay),
                   
                
                   Button("提交")
                       .Style(ThemeResource.AccentButtonStyle)
                       .BindCommand(vm?.Commit)
                       .Grid_Column(1)
                       .VerticalAlignment(VerticalAlignment.Bottom)
                       .HCenter().Margin(16,0)
               ).Grid_Row(2)
        ).Margin(24)
    ).Invoke(ContentInvoke);
    
    private DataTemplate GridViewTemplate => DataTemplate(() =>
        Grid(
            Image().Source().Bind("Path")
                .Stretch(UIMedia.Stretch.UniformToFill).HCenter().VCenter(),
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
