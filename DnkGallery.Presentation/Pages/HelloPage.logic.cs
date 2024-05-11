
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class HelloPage : BasePage<BindableHelloViewModel>, IBuildUI {
    public HelloPage() => BuildUI();
}

public partial record HelloViewModel : BaseViewModel {
    public IState<string> Text => UseState(() => string.Empty);
    
    public async Task Hello((string text,bool b) value) {
        await SetState(Text, _ => value.text);
    }
}
