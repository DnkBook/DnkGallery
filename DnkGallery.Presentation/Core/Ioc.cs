namespace DnkGallery.Presentation.Core;

public class Ioc {
    public void Init(IServiceProvider serviceProvider) {
        Service = serviceProvider;
    }
    public static IServiceProvider Service { get; set; }
}
