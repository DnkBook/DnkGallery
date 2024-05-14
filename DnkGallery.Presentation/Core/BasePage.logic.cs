using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml.Navigation;

namespace DnkGallery.Presentation.Core;

public abstract partial class BasePage<TViewModel> : BasePage where TViewModel : class, new() {
    protected BasePage() => DataContext = new TViewModel();
    protected TViewModel? vm => DataContext as TViewModel;
    
    protected virtual void OnNavigatedTo(NavigationEventArgs e) {
        base.OnNavigatedTo(e);
    }
}

// Because instances of this class are created with new instead of with a C# Markup 2 helper,
// derive this class from the UI type instead of from the C# Markup 2 type
public abstract partial class BasePage : UIControls.Page {
    const bool ShowTools =
#if DEBUG
        true;
#else
        false;
#endif
    
    protected IServiceProvider Service => Host.Services;

    protected static IHost? Host { get; set; }
    
    protected virtual void OnNavigatedTo(NavigationEventArgs e) {
        base.OnNavigatedTo(e);
    }
    
    
    protected BasePage() => NavigationCacheMode = NavigationCacheMode.Required;
}
