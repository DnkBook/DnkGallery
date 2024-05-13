namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    public MainPage() => BuildUI();
    
    
}

public partial record MainViewModel : BaseViewModel {
    public IState<string> Text => UseState(() => string.Empty);
    
    public async Task Hello((string text, bool b) value) {
        await SetState(Text, _ => value.text);
    }
    
    public IState<ObservableCollection<Category>> NavigationViewItems => UseState(() => Categories);
    
    public Frame? Frame;
    
    private readonly ObservableCollection<Category> Categories = [
        new Category() {
            Name = "Menu Item 1",
            Icon = Icon("Calender"),
            Children = new ObservableCollection<Category>() {
                new Category() {
                    Name = "Menu Item 2",
                    Icon = Icon("Calender"),
                    Children = new ObservableCollection<Category>() {
                        new Category() {
                            Name = "Menu Item 2",
                            Icon = Icon("Calender"),
                            Children = new ObservableCollection<Category>() {
                                new Category() { Name = "Menu Item 3", Icon = Icon("Calender"), },
                                new Category() { Name = "Menu Item 4", Icon = Icon("Calender"), }
                            }
                        }
                    }
                }
            }
        },
        new Category() {
            Name = "Menu Item 5",
            Icon = Icon("Calender"),
            Children = new ObservableCollection<Category>() {
                new Category() {
                    Name = "Menu Item 6",
                    Icon = Icon("Calender"),
                    Children = new ObservableCollection<Category>() {
                        new Category() { Name = "Menu Item 7", Icon = Icon("Calender"), },
                        new Category() { Name = "Menu Item 8", Icon = Icon("Calender"), }
                    }
                }
            }
        },
        new Category() { Name = "Menu Item 9", Icon = Icon("Calender"), }
    ];
}

public class Category {
    public string Name { get; set; }
    public IconElement Icon { get; set; }
    public ObservableCollection<Category> Children { get; set; }
}
