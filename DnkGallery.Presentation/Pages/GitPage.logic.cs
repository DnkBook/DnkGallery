namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GitPage : BasePage<BindableGitViewModel>, IBuildUI {
    public GitPage() => BuildUI();
    
}

public partial record GitViewModel : BaseViewModel {
   
}
