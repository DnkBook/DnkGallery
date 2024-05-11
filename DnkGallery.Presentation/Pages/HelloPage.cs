namespace DnkGallery.Presentation.Pages;

public partial class HelloPage {
    public void BuildUI() => Content(
        Grid(
            Rows(),
            StackPanel(
                TextBox()
                    .Bind(vm.Text, mode: BindingMode.TwoWay),
                Button("Hello")
                    .BindCommand(vm.Hello)
                    .CommandParameter(("Hello World, Dnk Gallery", false))
            ).Grid_Row(1).Center().Spacing(16),
            ExampleFooter().Grid_Row(2)
        )
    );
}
