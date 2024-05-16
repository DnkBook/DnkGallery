using DnkGallery.Model;
using Microsoft.UI.Xaml.Navigation;
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class AnaViewerPage : BasePage<BindableAnaViewViewModel>, IBuildUI {
    public AnaViewerPage() => BuildUI();
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        var ana = e.Parameter as Ana;
        await vm.Model.Ana.Update(_ => ana,CancellationToken.None);
        base.OnNavigatedTo(e);
    }
}

public partial record AnaViewViewModel : BaseViewModel {
    public IState<Ana> Ana => UseState(() => new Ana(default,default,default));
    
    
}
