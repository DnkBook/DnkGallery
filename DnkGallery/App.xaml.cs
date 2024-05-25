using DnkGallery.Model;
using DnkGallery.Model.Github;
using DnkGallery.Model.Services;
using DnkGallery.Presentation.Pages;
using Octokit;
using Uno.Extensions;
using Uno.Extensions.Configuration;
using Uno.Extensions.Hosting;
using Uno.Extensions.Localization;
// using Uno.Resizetizer;
using Uno.Toolkit.UI;
using Uno.UI;
using Microsoft.UI.Xaml;
#if WINDOWS
using Microsoft.UI.Composition.SystemBackdrops;
#endif

namespace DnkGallery;

public partial class App : UIXaml.Application {
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        this.InitializeComponent();
    }
    
    protected UIXaml.Window? MainWindow { get; private set; }
    protected IHost? Host { get; private set; }
    
    protected override void OnLaunched(UIXaml.LaunchActivatedEventArgs args) {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseLogging(configure: (context, logBuilder) => {
                    // Configure log levels for different categories of logging
                    logBuilder
                        .SetMinimumLevel(
                            context.HostingEnvironment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning)
                        
                        // Default filters for core Uno Platform namespaces
                        .CoreLogLevel(LogLevel.Warning);
                    
                    // Uno Platform namespace filter groups
                    // Uncomment individual methods to see more detailed logging
                    //// Generic Xaml events
                    //logBuilder.XamlLogLevel(LogLevel.Debug);
                    //// Layout specific messages
                    //logBuilder.XamlLayoutLogLevel(LogLevel.Debug);
                    //// Storage messages
                    //logBuilder.StorageLogLevel(LogLevel.Debug);
                    //// Binding related messages
                    //logBuilder.XamlBindingLogLevel(LogLevel.Debug);
                    //// Binder memory references tracking
                    //logBuilder.BinderMemoryReferenceLogLevel(LogLevel.Debug);
                    //// DevServer and HotReload related
                    //logBuilder.HotReloadCoreLogLevel(LogLevel.Information);
                    //// Debug JS interop
                    //logBuilder.WebAssemblyLogLevel(LogLevel.Debug);
                }, enableUnoLogging: true)
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                // Enable localization (see appsettings.json for supported languages)
                .UseLocalization()
                // Register Json serializers (ISerializer and ISerializer)
                .UseSerialization((context, services) => services
                    .AddContentSerializer(context))
                .ConfigureServices((context, services) => {
                    services.AddSingleton<Setting>();
                    services.AddSingleton<GitHubClient>(_ => new GitHubClient(new ProductHeaderValue("DnkGallery")) {
                        Credentials = new Credentials(_.GetService<Setting>()?.GitAccessToken,AuthenticationType.Anonymous)
                        
                    });
                    services.AddSingleton<IGitApi, GithubApi>();
                    services.AddKeyedSingleton<IGalleryService, LocalGalleryService>(Source.Local);
                    services.AddKeyedSingleton<IGalleryService, GitGalleryService>(Source.Git);
                })
            );
        MainWindow = builder.Window;
        
#if DEBUG
        MainWindow.EnableHotReload();
#endif
        // MainWindow.SetWindowIcon();
        
        Host = builder.Build();
        
        MainWindow.Content = new MainPage(MainWindow, Host);
#if WINDOWS
        // Ensure the current window is active
        MainWindow.ExtendsContentIntoTitleBar = true;
        MainWindow.SystemBackdrop = new UIMedia.MicaBackdrop() {
            Kind = MicaKind.BaseAlt
        };
        // Resources.Add("NavigationViewContentMargin", new Thickness(0, 48, 0, 0));
        // Resources.Add("WindowCaptionBackground", new UIMedia.SolidColorBrush(Colors.Transparent));
        // Resources.Add("WindowCaptionBackgroundDisabled", new UIMedia.SolidColorBrush(Colors.Transparent));
#endif
           
        // Do not repeat app initialization when the Window already has content,
        
        // Ensure the current window is active
        MainWindow.Activate();
    }
}
