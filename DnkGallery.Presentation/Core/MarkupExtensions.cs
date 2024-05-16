using Microsoft.UI.Xaml;
using UIElement = Microsoft.UI.Xaml.UIElement;

namespace DnkGallery.Presentation.Core;

static class MarkupExtensions {
    // Add any application-specific Markup extensions here
}

static class MarkupHelpers {
    internal static BitmapIcon Icon(string appAsset) => BitmapIcon(UriSource: new Uri($"ms-appx:///Assets/{appAsset}.png"));
    
    internal static Image Image(string appAsset) => CSharpMarkup.WinUI.Helpers.Image(Source: BitmapImage(new Uri($"ms-appx:///Assets/{appAsset}.png")));
    
    internal static TextBlock ExampleFooter() => TextBlock(Span("Built with C# Markup "), Span("2").FontSize(18), Span(" for Uno")).FontStyle().Italic()
        .Bottom().HCenter();
    
    
    public static Expander SettingsExpander(CSharpMarkup.WinUI.UIElement content,
        UIElement icon,
        string title,
        string description,
        UIElement right) {
        return Expander(
            content
        ).Header(
            Grid(
                HStack(
                    icon,
                    VStack(
                        TextBlock(title),
                        TextBlock(description).FontSize(12).Foreground(ThemeResource.TextFillColorSecondaryBrush)
                        ).VCenter()
                ).Spacing(24)
                .HorizontalAlignment(HorizontalAlignment.Left)
                .VCenter(),
                Grid(right)
                    .HorizontalAlignment(HorizontalAlignment.Right)
                    .VCenter()
            ).Height(64)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
                .VCenter().UI
        ).HorizontalAlignment(HorizontalAlignment.Stretch).HorizontalContentAlignment(HorizontalAlignment.Stretch);
    }
    // Add more application-specific Markup helpers here
}
