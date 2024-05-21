using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = CSharpMarkup.WinUI.DataTemplate;

namespace DnkGallery.Presentation.Pages;

partial class MainPage {
    private Grid AppTitleBar(string title) => Grid(Columns(Auto, Auto, Auto),
            FontIcon(new FontFamily("Segoe UI Emoji")).Glyph("🥵")
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
        Grid(AppTitleBar("DnkGallery"),
            NavigationView(
                Frame().Assign(out frame).Invoke(FrameInvoke))
                .PaneHeader(
                    HStack(
                        HyperlinkButton(FontIcon(FontSize:14).Glyph("\uE895"))
                            .ToolTipService_ToolTip("同步").Height(36).BindCommand(vm?.GitPull),
                        HyperlinkButton(FontIcon(FontSize:14).Glyph("\uE8AD"))
                            .Height(36)
                            .ToolTipService_ToolTip("推送").BindCommand(vm?.GitPush)
                    ).Height(44).Assign(out hstack).Margin(16,0)
                )
                .IsBackEnabled(true)
                
            .Assign(out navigationView)
            .Invoke(NavigationInvoke)
            // .MenuItemsSource()
            // .Bind(vm?.MenuItems)
            // .MenuItemTemplate(MenuItemTemplate)
            
        ).Invoke(GridInvoke)
    );
    
    
    
    private DataTemplate MenuItemTemplate => DataTemplate(
        () => NavigationViewItem()
            .Content().Bind("Name")
            .Icon().Bind("Icon")
            .MenuItemsSource().Bind("Children")
            .Tag().Bind("Name")
        );
}
