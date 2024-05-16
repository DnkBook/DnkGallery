using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Uno.Extensions;
using NavigationTransitionInfo = Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    private UIControls.Frame frame;
    private UIControls.NavigationView navigationView;
    
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
        
        if (navigationView.MenuItems.Count > 0) {
            var navigationViewMenuItem = navigationView.MenuItems[0] as UIControls.NavigationViewItem;
            Navigate(navigationViewMenuItem);
            navigationView.SelectedItem = navigationViewMenuItem;
        }
            
        
    }
    private void FrameInvoke(UIControls.Frame _) {
        frame.Navigated += (sender, args) => {
            navigationView.IsBackButtonVisible = frame.CanGoBack 
                ? UIControls.NavigationViewBackButtonVisible.Visible 
                : UIControls.NavigationViewBackButtonVisible.Collapsed;
        };
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
        
        navigationView.BackRequested += (sender, args) => {
            if (frame is { CanGoBack: true }) {
                Back();
            } else {
                navigationView.IsBackButtonVisible = UIControls.NavigationViewBackButtonVisible.Collapsed;
            }
        };
        
        navigationView.Loaded += async (sender, args) => {
            await LoadNavigation();
        };
        
        navigationView.ItemInvoked += (sender, args) => {
            if (args.IsSettingsInvoked) {
                frame?.Navigate(typeof(SettingPage),null, args.RecommendedNavigationTransitionInfo);
            } else if (args.InvokedItemContainer is UIControls.NavigationViewItem invokedItemContainer) {
                Navigate(invokedItemContainer,args.RecommendedNavigationTransitionInfo);
            }
        };
        
        
        // navigationView.SelectionChanged += (sender, args) => {
        //     if (args.IsSettingsSelected) {
        //         navigationView.Header = "设置";
        //         frame?.Navigate(typeof(SettingPage), null, args.RecommendedNavigationTransitionInfo);
        //     } else if (args.SelectedItemContainer != null) {
        //         var navigationTag = args.SelectedItemContainer.Tag as NavigationTag;
        //         navigationView.Header = (navigationTag.Parameter as Chapter)?.Name;
        //         frame?.Navigate(navigationTag.Page,
        //             navigationTag.Parameter,
        //             args.RecommendedNavigationTransitionInfo);
        //     }
        //     Debug.WriteLine(frame.BackStackDepth);
        // };
    }
    
    private void Navigate(UIControls.NavigationViewItem navigationViewItem, NavigationTransitionInfo? args = default) {
        var navigationTag = navigationViewItem.Tag as NavigationTag;
        navigationView.Header = (navigationTag?.Parameter as Chapter)?.Name;
        frame.Navigate(navigationTag?.Page, navigationTag?.Parameter, args);
    }
    
    private void Back() {
        var pageStackEntry = frame.BackStack[^1];
        if (pageStackEntry.SourcePageType == typeof(SettingPage)) {
            navigationView.SelectedItem = navigationView.SettingsItem;
        } else if(pageStackEntry.SourcePageType == typeof(GalleryPage)) {
            // TODO 树形结构带解决 解决方案：在Parameter添加新字段Anchors格式为数组[2024,05,17]查找时遍历
            navigationView.SelectedItem = navigationView.MenuItems
                .First(x => {
                    var navigationTag = (x as UIControls.NavigationViewItem)?.Tag as NavigationTag;
                    var page = navigationTag?.Page == pageStackEntry.SourcePageType;
                    var name = (navigationTag?.Parameter as Chapter)?.Name == (pageStackEntry.Parameter as Chapter)?.Name;
                    return page && name;
                });
        } else {
            navigationView.SelectedItem = navigationView.MenuItems
                .First(x => {
                    var navigationTag = (x as UIControls.NavigationViewItem)?.Tag as NavigationTag;
                    var page = navigationTag?.Page == pageStackEntry.SourcePageType;
                    return page;
                });
        }
        frame.GoBack();
    }
    
}

public record NavigationTag(Type Page, object? Parameter);

public partial record MainViewModel : BaseViewModel {
}
