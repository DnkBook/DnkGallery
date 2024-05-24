using Uno.Extensions;
using NavigationTransitionInfo = Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo;
using PageStackEntry = Microsoft.UI.Xaml.Navigation.PageStackEntry;

namespace DnkGallery.Presentation.Utils;

/// <summary>
/// 导航相关方法
/// </summary>
public static class Navigator {
    private static UIControls.NavigationView? _navigationView;
    private static UIControls.Frame? _frame;
    public static UIControls.NavigationView NavigationView => _navigationView ?? throw new InvalidOperationException();
    public static UIControls.Frame Frame => _frame ?? throw new InvalidOperationException();
    
    /// <summary>
    /// 导航初始化，在NavigationView加载完成后调用
    /// </summary>
    /// <param name="navigationView"></param>
    /// <param name="frame"></param>
    public static void Init(UIControls.NavigationView navigationView, UIControls.Frame frame) {
        _navigationView = navigationView;
        _frame = frame;
    }
    
    /// <summary>
    /// 创建导航条目
    /// </summary>
    /// <param name="name">唯一名称</param>
    /// <param name="content">导航条目显示内容</param>
    /// <param name="page">page页面</param>
    /// <param name="payload">附加信息</param>
    /// <param name="anchors">树形锚点用于定位导航条目，导航返回操作时用到</param>
    /// <param name="header">内容页的头部</param>
    /// <param name="icon">图标</param>
    /// <typeparam name="TParameter">附加信息类型，页面跳转时会传递过去</typeparam>
    /// <returns>导航条目</returns>
    public static UIControls.NavigationViewItem CreateNavigationViewItem<TParameter>(string name,
        string content,
        Type page,
        TParameter payload,
        bool hasChildren,
        string[] anchors,
        string? header = default,
        UIControls.Symbol icon = UIControls.Symbol.Calendar,
        NavigationViewItemType type = NavigationViewItemType.Main,
        UIControls.InfoBadge infoBadge = null
    ) {
        return new UIControls.NavigationViewItem {
            Content = content,
            Icon = new UIControls.SymbolIcon(icon),
            Tag = new NavigationTag<TParameter>(name, page, header ?? content,
                new NavigationParameter<TParameter>(name, anchors, payload, type)),
            HasUnrealizedChildren = hasChildren,
            InfoBadge = infoBadge
        };
    }
    
    /// <summary>
    /// 创建导航条目 无参数
    /// </summary>
    /// <param name="name">唯一名称</param>
    /// <param name="content">导航条目显示内容</param>
    /// <param name="page">page页面</param>
    /// <param name="header">内容页的头部</param>
    /// <param name="icon">图标</param>
    /// <returns>导航条目</returns>
    public static UIControls.NavigationViewItem CreateNavigationViewItem(string name,
        string content,
        Type page,
        string? header = default,
        UIControls.Symbol icon = UIControls.Symbol.Calendar) {
        return new UIControls.NavigationViewItem {
            Content = content,
            Icon = new UIControls.SymbolIcon(icon),
            Tag = new NavigationTag(name, page, header ?? content)
        };
    }
    
    /// <summary>
    /// 实体转导航条目
    /// </summary>
    /// <param name="model"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static UIControls.NavigationViewItem
        NavigationItemModelToNavigationViewItem<T>(NavigationItemModel<T> model) =>
        CreateNavigationViewItem(model.Name,
            model.Content,
            model.Page,
            model.Payload,
            model.HasChildren,
            model.Anchors,
            model.Header,
            model.Icon,
            model.Type,
            model.InfoBadge
            );
    
    /// <summary>
    /// 查找导航条目
    /// </summary>
    /// <param name="pageStackEntry">frame页面栈的Entry</param>
    /// <returns></returns>
    public static UIControls.NavigationViewItem? FindNavigationViewItem(PageStackEntry pageStackEntry) {
        var navigationParameter = pageStackEntry.Parameter as NavigationParameter;
        
        var menusItems =  navigationParameter.Type switch {
            NavigationViewItemType.Main => NavigationView.MenuItems,
            NavigationViewItemType.Footer => NavigationView.FooterMenuItems,
            _ => default
        };
        
        if (navigationParameter?.Anchors is null or { Length: 0 }) {
            return FindNavigationViewItem(menusItems,
                pageStackEntry.SourcePageType, navigationParameter.Name);
        }
        
        ICollection<object> items = [..menusItems];
        UIControls.NavigationViewItem? item = default;
        foreach (var navigationParameterAnchor in navigationParameter.Anchors) {
            item = FindNavigationViewItem(items, pageStackEntry.SourcePageType, navigationParameterAnchor);
            items = [..item?.MenuItems ?? []];
        }
        
        return item;
    }
    
    /// <summary>
    /// 查找导航条目
    /// </summary>
    /// <param name="menuItems">导航条目列表</param>
    /// <param name="page">页面</param>
    /// <param name="name">frame页面栈Entry</param>
    /// <returns></returns>
    private static UIControls.NavigationViewItem? FindNavigationViewItem(IEnumerable<object> menuItems, Type page,
        string? name) {
        return menuItems.FirstOrDefault(x => {
            var navigationTag = (x as UIControls.NavigationViewItem)?.Tag as NavigationTag;
            var pageEquals = navigationTag?.Page == page;
            var nameEquals = name is null || navigationTag?.Name == name;
            return pageEquals && nameEquals;
        }) as UIControls.NavigationViewItem;
    }
    
    /// <summary>
    /// 回退时导航条目同步选中，神金
    /// </summary>
    /// <param name="pageStackEntry">frame页面栈Entry</param>
    public static void Back(PageStackEntry pageStackEntry) {
        var findNavigationViewItem = FindNavigationViewItem(pageStackEntry);
        NavigationView.SelectedItem = findNavigationViewItem;
        var navigationTag = findNavigationViewItem?.Tag as NavigationTag;
        NavigationView.Header = navigationTag?.Header;
        Frame.Name = findNavigationViewItem?.Name;
        Frame.GoBack();
    }
    
    /// <summary>
    /// 是否是当前页
    /// </summary>
    /// <param name="navigationViewItem">导航条目</param>
    /// <returns></returns>
    public static bool IsCurrentPage(UIControls.NavigationViewItem navigationViewItem) {
        var navigationTag = navigationViewItem.Tag as NavigationTag;
        var pageEquals = navigationTag?.Page == Frame.SourcePageType;
        var nameEquals = navigationTag?.Name == Frame.Name;
        return pageEquals && nameEquals;
    }
    
    /// <summary>
    /// 导航
    /// </summary>
    /// <param name="navigationViewItem">导航条目</param>
    /// <param name="navigationTransitionInfo">动画参数</param>
    /// <typeparam name="TParameter">参数类型</typeparam>
    public static void Navigate<TParameter>(UIControls.NavigationViewItem navigationViewItem,
        NavigationTransitionInfo? navigationTransitionInfo = default) {
        var navigationTag = navigationViewItem.Tag as NavigationTag<TParameter>;
        NavigationView.Header = navigationTag?.Header;
        NavigationView.SelectedItem = navigationViewItem;
        Frame.Name = navigationTag?.Name;
        Frame.Navigate(navigationTag?.Page, navigationTag?.Parameter, navigationTransitionInfo);
    }
    
    /// <summary>
    /// 导航
    /// </summary>
    /// <param name="name">页面名称</param>
    /// <param name="page">页面</param>
    /// <param name="header">页头</param>
    /// <param name="parameter">参数</param>
    /// <param name="navigationTransitionInfo">动画参数</param>
    /// <typeparam name="TParameter">参数类型</typeparam>
    public static void Navigate<TParameter>(string name, Type page, string header,
        NavigationParameter<TParameter>? parameter = default,
        NavigationTransitionInfo? navigationTransitionInfo = default) {
        NavigationView.Header = header;
        Frame.Name = name;
        Frame.Navigate(page, parameter, navigationTransitionInfo);
    }
    
    /// <summary>
    /// 导航
    /// </summary>
    /// <param name="name">页面名称</param>
    /// <param name="page">页面</param>
    /// <param name="header">页头</param>
    /// <param name="navigationTransitionInfo">动画参数</param>
    public static void Navigate(string name, Type page, string header,
        NavigationTransitionInfo? navigationTransitionInfo = default) {
        NavigationView.Header = header;
        Frame.Name = name;
        Frame.Navigate(page, null, navigationTransitionInfo);
    }
    
    
    /// <summary>
    /// 设置菜单项目
    /// </summary>
    /// <param name="navigationItemModel"></param>
    /// <param name="hasChildrenPredicate"></param>
    /// <param name="addChildrenFunc"></param>
    /// <param name="parentNavigationViewItem"></param>
    public static async Task AddNavigationMenuItems<T>(IEnumerable<NavigationItemModel<T>> navigationItemModel,
        UIControls.NavigationViewItem? parentNavigationViewItem = default) {
        AddNavigationMenusItems(navigationItemModel,
            navigationViewItems => NavigationView.MenuItems.AddRange(navigationViewItems),
            parentNavigationViewItem);
    }
    
    
    /// <summary>
    /// 设置脚步项目
    /// </summary>
    /// <param name="navigationItemModel"></param>
    /// <param name="hasChildrenPredicate"></param>
    /// <param name="addChildrenFunc"></param>
    /// <param name="parentNavigationViewItem"></param>
    public static async Task AddFooterNavigationMenuItems<T>(IEnumerable<NavigationItemModel<T>> navigationItemModel,
        UIControls.NavigationViewItem? parentNavigationViewItem = default) {
        AddNavigationMenusItems(navigationItemModel,
            navigationViewItems => NavigationView.FooterMenuItems.AddRange(navigationViewItems),
            parentNavigationViewItem,NavigationViewItemType.Footer);
    }
    
    private static async Task AddNavigationMenusItems<T>(IEnumerable<NavigationItemModel<T>> navigationItemModel,
        Action<IEnumerable<UIControls.NavigationViewItem>> rootAddAction,
        UIControls.NavigationViewItem? parentNavigationViewItem = default,
        NavigationViewItemType type = NavigationViewItemType.Main) {
        var navigationTag = parentNavigationViewItem?.Tag as NavigationTag<T>;
        var parentAnchors = navigationTag?.Parameter.Anchors;
        
        var navigationViewItems = navigationItemModel.Select(item => {
            item.Anchors = [..parentAnchors ?? [], ..item.Anchors];
            item.Type = type;
            var navigationViewItem = NavigationItemModelToNavigationViewItem(item);
            return navigationViewItem;
        });
        
        if (parentNavigationViewItem is not null) {
            parentNavigationViewItem.MenuItems.AddRange(navigationViewItems);
        } else {
            rootAddAction.Invoke(navigationViewItems);
        }
    }
}

public class NavigationItemModel<T>() {
    public required string Name { get; set; }
    public required string Content { get; set; }
    public required string Header { get; set; }
    public bool HasChildren { get; set; }
    public UIControls.Symbol Icon { get; set; }
    public required Type Page { get; set; }
    public string[] Anchors { get; set; } = [];
    public required T Payload { get; set; }
    
    public UIControls.InfoBadge InfoBadge { get; set; }
    public NavigationViewItemType Type { get; set; }
}

public record NavigationTag(string Name, Type Page, string Header);

public record NavigationTag<T>(string Name, Type Page, string Header, NavigationParameter<T> Parameter)
    : NavigationTag(Name, Page, Header);

public record NavigationParameter(
    string Name,
    string[] Anchors,
    NavigationViewItemType Type = NavigationViewItemType.Main);

public record NavigationParameter<T>(
    string Name,
    string[] Anchors,
    T? Payload,
    NavigationViewItemType Type = NavigationViewItemType.Main) : NavigationParameter(Name, Anchors, Type);

public enum NavigationViewItemType {
    Main,
    Footer
}
