using DnkGallery.Model;
using Microsoft.UI.Xaml;
namespace DnkGallery.Presentation.Pages;

public partial class SettingPage {
    public void BuildUI() => Content(
        VStack(
            ComboBox()
                .Header("语录册源")
                .ItemsSource(Enum.GetValues(typeof(Source)))
                .SelectedItem()
                .Bind(vm?.Setting?.Source,BindingMode.TwoWay),
            TextBox()
                .Header("地址")
                .Text().Bind(vm?.Setting?.SourcePath,BindingMode.TwoWay)
                .HorizontalAlignment(HorizontalAlignment.Left).MaxWidth(500),
            HStack(
                Button("保存").BindCommand(vm?.Save),
                Button("取消").BindCommand(vm?.Cancel)
            ).Spacing(24)
        ).Margin(24).Spacing(24)
    );
}
