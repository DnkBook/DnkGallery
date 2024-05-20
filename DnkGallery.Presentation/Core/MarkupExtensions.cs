using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using UIElement = Microsoft.UI.Xaml.UIElement;

namespace DnkGallery.Presentation.Core;

internal static class MarkupExtensions {
    // Add any application-specific Markup extensions here
}

internal static class MarkupHelpers {
    internal static BitmapIcon Icon(string appAsset) => BitmapIcon(UriSource: new Uri($"ms-appx:///Assets/{appAsset}.png"));
    
    internal static UI.Xaml.Media.Imaging.BitmapImage ByteArrayConvertToBitmapImage(byte[] bytes) {
        var image = new UI.Xaml.Media.Imaging.BitmapImage();
        using var stream = new InMemoryRandomAccessStream();
        using var writer = new DataWriter(stream);
        writer.WriteBytes(bytes);
        writer.StoreAsync().GetAwaiter().GetResult();
        writer.FlushAsync().GetAwaiter().GetResult();
        writer.DetachStream();
        stream.Seek(0);
        image.SetSource(stream);
        return image;
    }
    
    internal static Image Image(string appAsset) => CSharpMarkup.WinUI.Helpers.Image(Source: BitmapImage(new Uri($"ms-appx:///Assets/{appAsset}.png")));
    
    internal static TextBlock ExampleFooter() => TextBlock(Span("Built with C# Markup "), Span("2").FontSize(18), Span(" for Uno")).FontStyle().Italic()
        .Bottom().HCenter();
    
    public static UIElement SettingsExpanderContent(UIElement left, UIElement? right = null) {
        return Grid(Columns(Auto, Star),
                HStack(left).VCenter().Margin(0, 0, 16, 0),
                right is not null ? HStack(right)
                        .HorizontalAlignment(HorizontalAlignment.Right)
                        .VCenter()
                        .Grid_Column(1)
                    : null)
            .HorizontalAlignment(HorizontalAlignment.Stretch);
    }
    
    public static Expander SettingsExpander(UIElement[] content,
        UIElement icon,
        string title,
        string description,
        UIElement? right = null) {
        return Expander(VStack(content)
            .Margin(44, 0, 44, 0)
            .Spacing(16)).Header(Grid(HStack(icon,
                        VStack(TextBlock(title),
                            TextBlock(description).FontSize(12).Foreground(ThemeResource.TextFillColorSecondaryBrush)).VCenter()).Spacing(24)
                    .HorizontalAlignment(HorizontalAlignment.Left)
                    .VCenter(),
                right is not null ? Grid(right)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .VCenter() : null).Padding(0, 16, 0, 16)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VCenter().UI).HorizontalAlignment(HorizontalAlignment.Stretch).HorizontalContentAlignment(HorizontalAlignment.Stretch);
    }
}
