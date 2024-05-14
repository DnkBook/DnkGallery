using DnkGallery.Model;
using Microsoft.UI.Xaml.Navigation;

namespace DnkGallery.Presentation.Pages;


[UIBindable]
public sealed partial class GalleryPage : BasePage<GalleryViewModel>, IBuildUI {
    private UIControls.ItemsView? itemsView;
    private UIControls.AnnotatedScrollBar? scrollBar;
    
    
    public GalleryPage() {
        BuildUI();
    }
    
    private void ItemsViewInvoke(UIControls.ItemsView obj) {
        // itemsView.VerticalScrollController = scrollBar.ScrollController;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e) {
        var parameter = e.Parameter;
        base.OnNavigatedTo(e);
        

    }
  
}

public partial record GalleryViewModel : BaseViewModel {
    public IState<ObservableCollection<Ana>> Anas => UseState(()=> new ObservableCollection<Ana>());
    

}

