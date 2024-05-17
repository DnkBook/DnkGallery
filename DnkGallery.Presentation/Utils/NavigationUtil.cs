using DnkGallery.Presentation.Pages;
using PageStackEntry = Microsoft.UI.Xaml.Navigation.PageStackEntry;

namespace DnkGallery.Presentation;

public static class NavigationUtil {
    public static UIControls.NavigationViewItem CreateNavigationViewItem<T>(
        string name,
        Type page,
        T payload,
        string[] anchors,
        string? header = default,
        UIControls.Symbol icon = UIControls.Symbol.Calendar
    ) {
        return new UIControls.NavigationViewItem {
            Content = name,
            Icon = new UIControls.SymbolIcon(icon),
            Tag = new NavigationTag<T>(page, header ?? name,
                new NavigationParameter<T>(name, anchors, payload)
            )
        };
    }
    
    public static UIControls.NavigationViewItem CreateNavigationViewItem(
        string name,
        Type page,
        string[] anchors,
        string? header = default,
        UIControls.Symbol icon = UIControls.Symbol.Calendar) {
        return new UIControls.NavigationViewItem {
            Content = name,
            Icon = new UIControls.SymbolIcon(icon),
            Tag = new NavigationTag(page, header ?? name,
                new NavigationParameter(name, anchors)
            )
        };
    }
    
    public static UIControls.NavigationViewItem FindNavigationViewItem<T>(
        UIControls.NavigationView navigationView,
        PageStackEntry pageStackEntry
    ) {
        var navigationParameter = pageStackEntry.Parameter as NavigationParameter<T>;
        // TODO 
        ICollection<object> items = [..navigationView.MenuItems];
        foreach (var navigationParameterAnchor in navigationParameter?.Anchors) {
            var navigationViewItem = FindNavigationViewItem<T>(items, pageStackEntry.SourcePageType, navigationParameterAnchor);
            if (navigationViewItem is null) {
                return navigationViewItem;
            }
            items = [..navigationViewItem.MenuItems];
        }
        return FindNavigationViewItem<T>(items, pageStackEntry.SourcePageType, navigationParameter.Name);
    }
    
    private static UIControls.NavigationViewItem FindNavigationViewItem<T>(ICollection<object> menuItems, Type page,
        string name) {
        return menuItems.FirstOrDefault(x => {
            var navigationTag = (x as UIControls.NavigationViewItem)?.Tag as NavigationTag<T>;
            var pageEquals = navigationTag?.Page == page;
            var nameEquals = navigationTag?.Parameter.Name == name;
            return pageEquals && nameEquals;
        }) as UIControls.NavigationViewItem;
    }
}
