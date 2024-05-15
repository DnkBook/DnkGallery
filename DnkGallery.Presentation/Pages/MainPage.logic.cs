using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Uno.Extensions;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    private UIControls.Frame? frame;
    private UIControls.NavigationView? navigationView;
    
    public MainPage(Window window, IHost host) {
        BuildUI();
        Host = host;
        MainWindow = window;
    }
    
    private async Task SetNavigationMenuItems(IList<object> menuItems, IEnumerable<Chapter> chapters) {
        var galleryService = Service.GetService<IGalleryService>();
        
        var childrenTasks = chapters.Select(async chapter => {
            var children = new UIControls.NavigationViewItem {
                Content = chapter.Name,
                Icon = new UIControls.SymbolIcon(UIControls.Symbol.Calendar),
                Tag = new NavigationTag(typeof(GalleryPage), chapter),
            };
            if (!chapter.HasChildren) {
                return children;
            }
            
            var list = await galleryService.Chapters(chapter.Dir);
            var navigationViewItems = list.Select(x => new UIControls.NavigationViewItem {
                Content = x.Name,
                Icon = new UIControls.SymbolIcon(UIControls.Symbol.Calendar),
                Tag = new NavigationTag(typeof(GalleryPage), x)
            }).ToList();
            children.MenuItems.AddRange(navigationViewItems);
            
            return children;
        });
        var childrenList = await Task.WhenAll(childrenTasks);
        menuItems.AddRange(childrenList);
    }
    
    private void NavigationInvoke(UIControls.NavigationView _) {
        navigationView.Expanding += async (sender, args) => {
            var navigationViewItem = args.ExpandingItem as UIControls.NavigationViewItem;
            var navigationTag = navigationViewItem.Tag as NavigationTag;
            var chapter = navigationTag.Parameter as Chapter;
            var galleryService = Service.GetService<IGalleryService>();
            var chapters = await galleryService.Chapters(chapter.Dir);
            
            await SetNavigationMenuItems(navigationViewItem.MenuItems, chapters);
        };
        
        navigationView.Loaded += async (sender, args) => {
            var galleryService = Service.GetService<IGalleryService>();
            var chapters = await galleryService.Chapters("C:\\Users\\NianChen\\Pictures\\dnk\\dnkFuns\\dnkBook");
            await SetNavigationMenuItems(navigationView.MenuItems, chapters);
            
            frame.Navigate(typeof(HelloPage));
        };
        
        navigationView.ItemInvoked += (sender, args) => {
            if (args.IsSettingsInvoked) {
                frame.Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            } else if (args.InvokedItemContainer != null) {
                var navigationTag = args.InvokedItemContainer.Tag as NavigationTag;
                frame.Navigate(navigationTag.Page,
                    navigationTag.Parameter,
                    args.RecommendedNavigationTransitionInfo);
            }
        };
        
        navigationView.SelectionChanged += (sender, args) => {
            if (args.IsSettingsSelected) {
                frame.Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            } else if (args.SelectedItemContainer != null) {
                var navigationTag = args.SelectedItemContainer.Tag as NavigationTag;
                frame.Navigate(navigationTag.Page,
                    navigationTag.Parameter,
                    args.RecommendedNavigationTransitionInfo);
            }
        };
    }
}

public record NavigationTag(Type Page, object Parameter);

public partial record MainViewModel : BaseViewModel {
}
