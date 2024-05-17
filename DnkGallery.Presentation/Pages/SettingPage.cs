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
             ).Spacing(2).HorizontalAlignment(HorizontalAlignment.Stretch)
            
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
                SettingsExpanderContent(TextBlock("源地址"), 
                    TextBox()
                        .MaxWidth(300)
                    .Text().Bind(vm?.Setting?.SourcePath, BindingMode.TwoWay))
            ], SymbolIcon(UIControls.Symbol.Folder),
            "语录册源",
            "使用本地源或者从Git上获取")
    ];
}
