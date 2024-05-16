using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class SettingPage : BasePage<BindableSettingViewModel>, IBuildUI {
    public SettingPage() => BuildUI();
    
    
}

public partial record SettingViewModel : BaseViewModel {
    public IState<Setting> Setting => UseState(() => Service.GetService<Setting>() ?? new Setting());
    
    public async Task Save() {
        var setting = await Setting;
        await Service.GetService<Setting>().SaveAsync(setting);
    }
    public async Task Cancel() {
        
    }
}


