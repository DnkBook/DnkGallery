using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = Microsoft.UI.Xaml.DataTemplate;

namespace DnkGallery.Presentation.Pages;

public partial class GalleryPage {
    public void BuildUI() => 
        Content(
        Grid(
            Columns(Star, Auto),
            GridView()
                .ItemsSource().Bind(vm?.Anas)
                .ItemTemplate(GridViewTemplate)
                .Assign(out gridView),
            ContentDialog(
                VStack(
                    Image()
                        .Height(300).Width(400)
                        .Source().Bind(vm?.SaveData?.Image),
                        TextBlock().Bind(vm?.Chapter?.Dir,convert:(string source) => $"保存路径：{source}"),
                        TextBox().Header("文件名").Bind(vm?.SaveData?.FileName),
                        CheckBox("添加到Git")
                    ).Spacing(24)
                )
                .XamlRoot(MainWindow.Content.XamlRoot)
                .Title().Bind(vm?.Chapter?.Name, convert:(string title) => $"保存到 {title}")
                .PrimaryButtonText("保存")
                .PrimaryButtonCommand().Bind(vm?.Save)
                .DefaultButton(UIControls.ContentDialogButton.Primary)
                .SecondaryButtonText("取消")
                .Assign(out saveDialog)
        )
    ).Invoke(ContentInvoke);
    
    private DataTemplate GridViewTemplate => DataTemplate(() =>
        Grid(
            Image().Source().Bind("ImageBytes", convert:  (byte[] bytes) =>  ByteArrayConvertToBitmapImage(bytes))
                .Stretch(Stretch.UniformToFill).HCenter().VCenter(),
            VStack(
                    TextBlock()
                        .Text().Bind("Name").HCenter().VCenter().FontSize(16)
                )
                .Height(32)
                .VerticalAlignment(VerticalAlignment.Bottom)
                .Width(300)
                .Background(ThemeResource.AcrylicBackgroundFillColorDefaultBrush)
                .Opacity(0.75)
        ).Width(300).Height(200).Invoke(GridViewItemInvoke)
    );
}
