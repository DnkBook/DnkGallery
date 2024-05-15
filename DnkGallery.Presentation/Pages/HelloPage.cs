namespace DnkGallery.Presentation.Pages;

public partial class HelloPage {
    public void BuildUI() => Content(
        Grid(
            TextBlock()
                .Text("Hello DnkGallery")
            )
    );
}
