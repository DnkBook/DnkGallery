using DnkGallery.Model;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using UIElement = Microsoft.UI.Xaml.UIElement;

namespace DnkGallery.Presentation.Pages;

public partial class SettingPage {
    public static readonly string Header = "设置";
    public void BuildUI() => Content(
        VStack(
            TextBlock("基本设置")
                .FontWeight(FontWeights.Bold),
             VStack(
                 BaiscSettingItems()
             ).Spacing(2).HorizontalAlignment(HorizontalAlignment.Stretch),
             
             TextBlock("Git设置")
                 .FontWeight(FontWeights.Bold),
             VStack(
                 GitSettingItems()
             ).Spacing(2).HorizontalAlignment(HorizontalAlignment.Stretch),
            
             Button("保存").BindCommand(vm?.Save)
        ).Margin(24).Spacing(12)
    );
    
    private UIElement[] BaiscSettingItems() => [
        SettingsExpander([
                SettingsExpanderContent(TextBlock("源类型"), 
                    ComboBox()
                    .ItemsSource(Enum.GetValues(typeof(Source)))
                    .Width(300)
                    .SelectedItem()
                    .Bind(vm?.Setting?.Source, BindingMode.TwoWay)),
                SettingsExpanderContent(TextBlock("本地地址"), 
                    TextBox()
                        .MaxWidth(300)
                    .Text().Bind(vm?.Setting?.LocalPath, BindingMode.TwoWay)),
                SettingsExpanderContent(TextBlock("Git仓库"),
                    TextBox()
                        .MaxWidth(300)
                        .Text().Bind(vm?.Setting?.GitRepos, BindingMode.TwoWay)),
            ], SymbolIcon(UIControls.Symbol.Folder),
            "语录册源",
            "使用本地源或者从Git上获取")
    ];
    
    private UIElement[] GitSettingItems() => [
        SettingsExpander([
                SettingsExpanderContent(TextBlock("Git Access Token"),
                    TextBox()
                        .AcceptsReturn(true)
                        .TextWrapping(TextWrapping.Wrap)
                        .MaxHeight(200)
                        .ScrollViewer_VerticalScrollBarVisibility(UIControls.ScrollBarVisibility.Auto)
                        .MaxWidth(300)
                        .Text().Bind(vm?.Setting?.GitAccessToken, BindingMode.TwoWay)),
            ], SymbolIcon(UIControls.Symbol.Help),
            "Git参数",
            "Git参数设置")
    ];
}
