using Microsoft.UI.Xaml;
using DataTemplate = CSharpMarkup.WinUI.DataTemplate;

namespace DnkGallery.Presentation.Pages;

partial class MainPage{
    private Grid AppTitleBar(string title) => Grid(
            Columns(Auto, Auto),
            Image().Width(16).Height(16)
                .HorizontalAlignment(HorizontalAlignment.Left)
                .VCenter(),
            TextBlock()
                .Margin(12, 0, 0, 0)
                .Grid_Column(1)
                .Text(title)
                .VCenter().Margin(28, 0, 0, 0)
        )
        .Height(48)
        .Margin(48, 0, 0, 0)
        .VerticalAlignment(VerticalAlignment.Top)
        .Padding(0);
    
    public void BuildUI() => Content(
        Grid(
            AppTitleBar("DnkGallery"),
            NavigationView(
                    Frame()
                ).MenuItemsSource().Bind(vm.NavigationViewItems)
            .MenuItemTemplate(MenuItemTemplate)
        )
    );
    

    
    private DataTemplate MenuItemTemplate => DataTemplate(() => HStack(
        FontIcon().Glyph("\uE787"),
        TextBlock().Text().Bind()
    ).Spacing(24));
}
