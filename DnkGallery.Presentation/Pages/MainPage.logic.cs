namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    public MainPage() {
        BuildUI();
    }
    
    
}

public partial record MainViewModel : BaseViewModel {
    public IState<string> Text => UseState(() => string.Empty);
    
    public async Task Hello((string text, bool b) value) {
        await SetState(Text, _ => value.text);
    }
    
    public IState<ObservableCollection<dynamic>> NavigationViewItems => UseState(() => categories);
    
    private readonly ObservableCollection<dynamic> categories = [
         "Menu Item 1",
         "Menu Item 2",
    ];
}
