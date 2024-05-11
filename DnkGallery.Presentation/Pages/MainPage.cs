using Uno.Extensions;

namespace DnkGallery.Presentation.Pages;

partial class MainPage {
    public void BuildUI() => Content(
        NavigationView(
            Frame()
        ).MenuItemsSource(categories)
            .MenuItemTemplate(NavigationViewTemplate())
    );
    
    private DataTemplate NavigationViewTemplate() => DataTemplate(x => {
            var category = x.Get<Category>();
            NavigationViewItem(Content: category.Name, Icon: Icon(category.Icon)).MenuItemsSource(category.Children);
        }
    );
    
    public class Category {
        public String Name { get; set; }
        public String Icon { get; set; }
        public ObservableCollection<Category> Children { get; set; }
    }
    
    private readonly ObservableCollection<Category> categories = [
        new Category {
            Name = "Menu Item 1",
            Icon = "Icon",
            Children = [
                new Category {
                    Name = "Menu Item 2",
                    Icon = "Icon",
                    Children = [
                        new Category {
                            Name = "Menu Item 2",
                            Icon = "Icon",
                            Children = [
                                new Category { Name = "Menu Item 3", Icon = "Icon" },
                                new Category { Name = "Menu Item 4", Icon = "Icon" }
                            ]
                        }
                    ]
                }
            ]
        },
        new Category() {
            Name = "Menu Item 5",
            Icon = "Icon",
            Children = [
                new Category {
                    Name = "Menu Item 6",
                    Icon = "Icon",
                    Children = [
                        new Category { Name = "Menu Item 7", Icon = "Icon" },
                        new Category { Name = "Menu Item 8", Icon = "Icon" }
                    ]
                }
            ]
        },
        new Category { Name = "Menu Item 9", Icon = "Icon" }
    ];
}
