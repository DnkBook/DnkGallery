using CSharpMarkup.WinUI.Uno.Toolkit;
using DnkGallery.Model;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;

namespace DnkGallery.Presentation.Pages;

public partial class SettingPage {
    public void BuildUI() => Content(
        VStack(
            TextBlock("基本设置").FontWeight(FontWeights.Bold),
            VStack (
                
                SettingsExpander(
                    HStack(
                        TextBlock("源地址").VCenter(),
                        TextBox()
                            .Text().Bind(vm?.Setting?.SourcePath, BindingMode.TwoWay)
                            .HorizontalAlignment(HorizontalAlignment.Left).MaxWidth(500)
                    ).Spacing(24),
                    SymbolIcon(UIControls.Symbol.Folder),
                    "语录册源",
                    "使用本地源或者从Git上获取",
                    ComboBox()
                        .ItemsSource(Enum.GetValues(typeof(Source)))
                        .SelectedItem()
                        .Bind(vm?.Setting?.Source, BindingMode.TwoWay)
                        .VCenter()
                )
                // ,
                // HStack(
                //     Button("保存").BindCommand(vm?.Save),
                //     Button("取消").BindCommand(vm?.Cancel)
                // ).Spacing(24)
            ).Spacing(2).HorizontalAlignment(HorizontalAlignment.Stretch)
            ).Margin(24).Spacing(12)
    );
}
