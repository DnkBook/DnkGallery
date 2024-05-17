using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

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
            setting.SettingChanged += (_, _) => { DispatcherQueue.TryEnqueue(async () => await LoadNavigation()); };
        }
        
        MainWindow = window;
    }
    
    private async Task LoadNavigation() {
        var galleryService = Service.GetService<IGalleryService>()!;
        var chapters = await galleryService.Chapters(Service.GetService<Setting>()!.SourcePath);
        var navigationItemModels = chapters.Select(CreateChapterNavigationItemModel);
        
        navigationView.MenuItems.Clear();
        await Navigater.AddNavigationMenuItems(navigationItemModels,
            model => model.Payload.HasChildren,
            async model => {
                var payloadDir = model.Payload.Dir;
                var list = await galleryService.Chapters(payloadDir);
                var itemModels = list.Select(CreateChapterNavigationItemModel);
                return itemModels;
            });
        
        // 默认导航到第一个菜单
        if (navigationView.MenuItems is { Count: > 0 } && navigationView.MenuItems[0] is UIControls.NavigationViewItem navigationViewMenuItem) {
            Navigater.Navigate<Chapter>(navigationViewMenuItem);
        }
    }
    
    private void FrameInvoke(UIControls.Frame _) {
        frame.Navigated += (_, _) => {
            navigationView.IsBackButtonVisible = frame.CanGoBack
                ? UIControls.NavigationViewBackButtonVisible.Visible
                : UIControls.NavigationViewBackButtonVisible.Collapsed;
        };
    }
    
    private static NavigationItemModel<Chapter> CreateChapterNavigationItemModel(Chapter chapter) =>
        new() {
            Name = chapter.Name,
            Content = chapter.Name,
            Header = chapter.Name,
            Page = typeof(GalleryPage),
            Anchors = chapter.Anchors,
            Icon = UIControls.Symbol.Calendar,
            Payload = chapter
        };
    
    private void NavigationInvoke(UIControls.NavigationView _) {
        
        Navigater.Init(navigationView, frame);
        
        navigationView.Loaded += async (sender, args) => {
            await LoadNavigation();
        };
        
        navigationView.Expanding += async (sender, args) => {
            var navigationViewItem = args.ExpandingItem as UIControls.NavigationViewItem;
            if (navigationViewItem?.Tag is NavigationTag<Chapter> { Parameter.Payload.Dir: not null } navigationTag) {  
                var galleryService = Service.GetService<IGalleryService>()!;
                var chapters = await galleryService.Chapters(navigationTag.Parameter.Payload.Dir);
                
                var navigationItemModels = chapters.Select(CreateChapterNavigationItemModel);
                navigationViewItem.MenuItems.Clear();
                await Navigater.AddNavigationMenuItems(navigationItemModels,
                    model => model.Payload.HasChildren,
                    async model => {
                        var payloadDir = model.Payload.Dir;
                        var list = await galleryService.Chapters(payloadDir);
                        var itemModels = list.Select(CreateChapterNavigationItemModel);
                        return itemModels;
                    }, navigationViewItem);
            }else{
                // 补充其它的展开策略
            }
        };
        
        navigationView.BackRequested += (sender, args) => {
            if (frame is { CanGoBack: true }) {
                Back();
            } else {
                navigationView.IsBackButtonVisible = UIControls.NavigationViewBackButtonVisible.Collapsed;
            }
        };
        
        navigationView.ItemInvoked += (sender, args) => {
            // 防止重复点击
            if (args.InvokedItemContainer is UIControls.NavigationViewItem invokedItemContainer && Navigater.IsCurrentPage(invokedItemContainer)) {
                return;
            }
            //设置导航写死了
            if (args.IsSettingsInvoked) {
                Navigater.Navigate(SettingPage.Header, typeof(SettingPage), SettingPage.Header, args.RecommendedNavigationTransitionInfo);
            } else switch (args.InvokedItemContainer) {
                    case UIControls.NavigationViewItem { Tag: NavigationTag<Chapter> } chapterNavigationViewItem:
                        Navigater.Navigate<Chapter>(chapterNavigationViewItem, args.RecommendedNavigationTransitionInfo);
                        break;
                    case UIControls.NavigationViewItem navigationViewItem:
                        Navigater.Navigate<object>(navigationViewItem, args.RecommendedNavigationTransitionInfo);
                        break;
                }
        };
        
    }
    
    /// <summary>
    /// 页面回退，有点神金
    /// </summary>
    private void Back() {
        var pageStackEntry = frame.BackStack[^1];
        if (pageStackEntry.SourcePageType == typeof(SettingPage)) {
            navigationView.Header = SettingPage.Header;
            navigationView.SelectedItem = navigationView.SettingsItem;
            frame.GoBack();
        }else {
            Navigater.Back(pageStackEntry);
        }

    }
}

public partial record MainViewModel : BaseViewModel {
}
