using DnkGallery.Model;
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class SettingPage : BasePage<BindableSettingViewModel>, IBuildUI {
    public SettingPage() => BuildUI();
    
    
}

public partial record SettingViewModel : BaseViewModel {
    public IState<Setting> Setting => UseState(() => Settings);
    
    public async Task Save() {
        try {
            var setting = await Setting;
            await Settings.SaveAsync(setting);
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success,SettingPage.Header,"保存成功");
        } catch (Exception e) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error,SettingPage.Header,e.Message);
        }

    }
    public async Task Cancel() {
        
    }
}


