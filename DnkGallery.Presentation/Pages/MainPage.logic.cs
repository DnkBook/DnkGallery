using Windows.Foundation;
using DnkGallery.Model;
using DnkGallery.Model.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using FrameworkElement = Microsoft.UI.Xaml.FrameworkElement;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    private UIControls.Frame frame;
    private UIControls.NavigationView navigationView;
    private UIControls.CommandBar commandBar;
    private UIControls.StackPanel hstack;
    public MainPage(Window window, IHost host) {
        MainWindow = window;
        BuildUI();
        Host = host;
        Ioc.Service = host.Services;
        var setting = Ioc.Service.GetService<Setting>();
        setting?.Load();
        if (setting != null) {
            setting.SettingChanged += (_, _) => { DispatcherQueue.TryEnqueue(async () => await LoadNavigation()); };
        }
#if WINDOWS        
        // hstack.Loaded += (sender, args) => AllowsClickThrough(hstack);
#endif
    }
    
    private void AllowsClickThrough(FrameworkElement frameworkElement) {
        var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(MainWindow.AppWindow.Id);
        // textbox on titlebar area
        var transformTxtBox = frameworkElement.TransformToVisual(null);
        var bounds = transformTxtBox.TransformBounds(new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
        
        // Windows.Graphics.RectInt32[] rects defines the area which allows click throughs in custom titlebar
        // it is non dpi-aware client coordinates. Hence, we convert dpi aware coordinates to non-dpi coordinates
        // var scale = WindowHelper.GetRasterizationScaleForElement(MainWindow);
        
        var transparentRect = new Windows.Graphics.RectInt32 {
            X = (int)Math.Round(bounds.X),
            Y = (int)Math.Round(bounds.Y),
            Width = (int)Math.Round(bounds.Width),
            Height = (int)Math.Round(bounds.Height)
        };
        
        var rects = new[] { transparentRect };
        nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
        
    }
    private async Task LoadNavigation() {
        var galleryService = Service.GetKeyedService<IGalleryService>(Settings.Source)!;
        var chapters = await galleryService.Chapters(Settings.SourcePath);
        var navigationItemModels = chapters.Select(CreateChapterNavigationItemModel);
        
        navigationView.MenuItems.Clear();
        await Navigater.AddNavigationMenuItems(navigationItemModels);
        
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
            HasChildren = chapter.HasChildren,
            Page = typeof(GalleryPage),
            Anchors = chapter.Anchors,
            Icon = UIControls.Symbol.Calendar,
            Payload = chapter
        };
    
    private static IEnumerable<NavigationItemModel<object>> CreateFooterNavigationItemModels() =>
        [ new NavigationItemModel<object>() {
            Name = GitPage.Header,
            Content = GitPage.Header,
            Header = GitPage.Header,
            Page = typeof(GitPage),
            Icon = UIControls.Symbol.Remote,
            Payload = default,
        }];
    
    private void NavigationInvoke(UIControls.NavigationView _) {
        
        Navigater.Init(navigationView, frame);
        
        navigationView.Loaded += async (sender, args) => {
            await Navigater.AddFooterNavigationMenuItems(CreateFooterNavigationItemModels());
            await LoadNavigation();
        };
        
        navigationView.Expanding += async (sender, args) => {
            var navigationViewItem = args.ExpandingItem as UIControls.NavigationViewItem;
            if (navigationViewItem?.MenuItems.Count > 0)
                return;
            if (navigationViewItem?.Tag is NavigationTag<Chapter> { Parameter.Payload.Dir: not null } navigationTag) {
                var galleryService = Service.GetKeyedService<IGalleryService>(Settings.Source)!;
                var chapters = await galleryService.Chapters(navigationTag.Parameter.Payload.Dir);
                
                var navigationItemModels = chapters.Select(CreateChapterNavigationItemModel);
                await Navigater.AddNavigationMenuItems(navigationItemModels, navigationViewItem);
            } else {
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
            } else
                switch (args.InvokedItemContainer) {
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
        } else {
            Navigater.Back(pageStackEntry);
        }
        
    }
    
}

public partial record MainViewModel : BaseViewModel {
}
