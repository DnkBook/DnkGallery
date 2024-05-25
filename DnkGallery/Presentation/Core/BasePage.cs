namespace DnkGallery.Presentation.Core;

partial class BasePage
{
    protected new Page Content(UIElement content) => this.Content(ShowTools,
        content.UI
    );
}
