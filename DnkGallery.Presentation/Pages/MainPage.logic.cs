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
        Ioc.Service = host.Services;
        var setting = Ioc.Service.GetService<Setting>();
        setting?.Load();
        if (setting != null) {
            setting.SettingChanged += (_, _) => {
                DispatcherQueue.TryEnqueue(async () => await LoadNavigation());
            };
        }
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
        menuItems.Clear();
        menuItems.AddRange(childrenList);
    }
    
    private async Task LoadNavigation() {
        var galleryService = Service.GetService<IGalleryService>();
        var chapters = await galleryService.Chapters(Service.GetService<Setting>().SourcePath);
        await SetNavigationMenuItems(navigationView.MenuItems, chapters);
        
        if (navigationView.MenuItems.Count > 0)
            navigationView.SelectedItem = navigationView.MenuItems[0];
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
            await LoadNavigation();
        };
        
        
        navigationView.ItemInvoked += (sender, args) => {
            if (args.IsSettingsInvoked) {
                navigationView.Header = "设置";
                frame.Navigate(typeof(SettingPage), null, args.RecommendedNavigationTransitionInfo);
            } else if (args.InvokedItemContainer != null) {
                var navigationTag = args.InvokedItemContainer.Tag as NavigationTag;
                navigationView.Header = (navigationTag.Parameter as Chapter)?.Name;
                frame.Navigate(navigationTag.Page,
                    navigationTag.Parameter,
                    args.RecommendedNavigationTransitionInfo);
            }
        };
        
        navigationView.SelectionChanged += (sender, args) => {
            if (args.IsSettingsSelected) {
                navigationView.Header = "设置";
                frame.Navigate(typeof(SettingPage), null, args.RecommendedNavigationTransitionInfo);
            } else if (args.SelectedItemContainer != null) {
                var navigationTag = args.SelectedItemContainer.Tag as NavigationTag;
                navigationView.Header = (navigationTag.Parameter as Chapter)?.Name;
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
