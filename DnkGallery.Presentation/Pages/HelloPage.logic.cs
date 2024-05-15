
namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class HelloPage : BasePage<BindableHelloViewModel>, IBuildUI {
    public HelloPage() => BuildUI();
}

public partial record HelloViewModel : BaseViewModel {
}
