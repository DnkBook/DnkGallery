using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Navigation;
using Uno.Extensions;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class GalleryPage : BasePage<GalleryViewModel>, IBuildUI {
    private UIControls.GridView itemsView;
    private UIControls.AnnotatedScrollBar? scrollBar;
    
    
    public GalleryPage() {
        BuildUI();
    }
    
    private void ContentInvoke(UIControls.Page obj) {
        obj.Loaded += (sender, args) => {
            // itemsView = scrollBar?.ScrollController;
            itemsView.ItemTemplate = ItemViewTemplate;
        };
        
    }
    
    protected override async void OnNavigatedTo(NavigationEventArgs e) {
        var chapter = e.Parameter as Chapter;
        var galleryService = Service.GetService<IGalleryService>()!;
        var anas = await galleryService.Anas(chapter);
        await vm.Anas.Update(_ => anas.ToObservableCollection(), CancellationToken.None);
        itemsView.ItemsSource = anas;
        base.OnNavigatedTo(e);
    }
    
}

public partial record GalleryViewModel : BaseViewModel {
    public IState<ObservableCollection<Ana>> Anas => UseState(()=> new ObservableCollection<Ana>());

}
