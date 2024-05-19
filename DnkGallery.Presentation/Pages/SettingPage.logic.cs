using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class SettingPage : BasePage<BindableSettingViewModel>, IBuildUI {
    public SettingPage() => BuildUI();
    
    
}

public partial record SettingViewModel : BaseViewModel {
    public IState<Setting> Setting => UseState(() => Settings);
    
    public async Task Save() {
        var setting = await Setting;
        await Settings.SaveAsync(setting);
    }
    public async Task Cancel() {
        
    }
}


