using Windows.System.Threading;
using DnkGallery.Model;
using DnkGallery.Model.Github;
using DnkGallery.Model.Services;
using LibGit2Sharp;

namespace DnkGallery.Presentation.Pages;

[UIBindable]
public sealed partial class MainPage : BasePage<BindableMainViewModel>, IBuildUI {
    private UIControls.Frame frame;
    private UIControls.NavigationView navigationView;
    private UIControls.CommandBar commandBar;
    private UIControls.InfoBadge gitInfoBadge;
    private UIControls.ContentDialog pushDialog;
    private UIControls.HyperlinkButton pushHyperlinkButton;
    private UIControls.ComboBox branchesComboBox;
    private UIControls.Button branchesComboBoxFlyoutCancel;
    private UIControls.Button branchesComboBoxFlyoutConfirm;
    
    public MainPage(UIXaml.Window window, IHost host) {
        MainWindow = window;
        Host = host;
        Ioc.Service = host.Services;
        BuildUI();
        var setting = Ioc.Service.GetService<Setting>();
        setting?.Load();
        if (setting != null) {
            setting.SettingChanged += (_, _) => { DispatcherQueue.TryEnqueue(async () => await LoadNavigation()); };
        }
        
#if WINDOWS
        // hstack.Loaded += (sender, args) => AllowsClickThrough(hstack);
#endif
        Loaded += PageLoaded;
    }
    
    private async void PageLoaded(object sender, UIXaml.RoutedEventArgs e) {
        CreateAutoSyncCheck();
        pushHyperlinkButton.Tapped += (sender, args) => OpenPushDialog();
        branchesComboBox.TextSubmitted += BranchesComboBoxTextSubmitted;
        branchesComboBoxFlyoutCancel.Click += (o, eventArgs) => { CloseBranchesComboboxFlyout(); };
        branchesComboBoxFlyoutConfirm.Click += CreateBranch;
        branchesComboBox.DropDownOpened += (o, o1) => { GetBranches(); };
        // branchesComboBox.SelectionChanged += (o, args) => {
        //     var argsAddedItem = args.AddedItems[0] as Branch;
        //     SwitchBranch(argsAddedItem.FriendlyName);
        // };
        await GetBranches();
        // branchesComboBox.Loaded += async (o, args) => {
        
        var gitApi = Service.GetService<IGitApi>()!;
        var branch = await gitApi.Branch(Settings.LocalPath);
        await vm.Model.BranchName.Update(_ => branch.FriendlyName, CancellationToken.None);
        // };
    }
    
    private async Task SwitchBranch(string selection) {
        try {
            var gitApi = Service.GetService<IGitApi>()!;
            var branches = await vm.Model.Branches;
            var branch = branches.FirstOrDefault(x => x.FriendlyName == selection);
            await gitApi.Checkout(Settings.LocalPath, branch);
            
            await vm?.Model.Status();
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success, GitPage.Header, $"切换{selection}分支成功");
            
            branchesComboBox.Text = selection;
            await vm.Model.BranchName.Update(_ => selection, CancellationToken.None);
            
        } catch (Exception ex) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, ex.Message);
        }
    }
    
    private async Task GetBranches() {
        var gitApi = Service.GetService<IGitApi>()!;
        var branchCollection = await gitApi.Branches(Settings.LocalPath);
        await vm.Model.Branches.Update(_ => [..branchCollection], CancellationToken.None);
    }
    
    private void CloseBranchesComboboxFlyout() {
        branchesComboBox.ContextFlyout.Hide();
    }
    
    private async void CreateBranch(object sender, UIXaml.RoutedEventArgs e) {
        try {
            var gitApi = Service.GetService<IGitApi>()!;
            var branchName = await vm.Model.CreateBranchName;
            var branch = await gitApi.CreateBranch(Settings.LocalPath, branchName);
            await vm.Model.Branches.AddAsync(branch);
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success, GitPage.Header, $"创建{branchName}分支成功");
            CloseBranchesComboboxFlyout();
            
            await SwitchBranch(branch.FriendlyName);
        } catch (Exception ex) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, ex.Message);
        }
    }
    
    
    private void GridInvoke(UIControls.Grid obj) {
        InfoBarManager.Init(MainWindow, obj);
    }
    
    private async Task OpenPushDialog() {
        var gitApi = Service.GetService<IGitApi>()!;
        var commits = await gitApi.BeingPushedCommits(Settings.LocalPath);
        await vm.Model.BeingPushedCommits.Update(_ => [..commits], CancellationToken.None);
        await pushDialog.ShowAsync();
    }
    
    private async void BranchesComboBoxTextSubmitted(UIControls.ComboBox sender,
        UIControls.ComboBoxTextSubmittedEventArgs args) {
        args.Handled = true;
        var text = args.Text;
        if (text == await vm.Model.BranchName) {
            return;
        }
        var asyncEnumerable = await vm.Model.Branches;
        var branch = asyncEnumerable.FirstOrDefault(x => x.FriendlyName == text);
        if (branch is not null) {
            await SwitchBranch(text);
        } else {
            await vm.Model.CreateBranchName.Update(_ => text, CancellationToken.None);
            sender.ContextFlyout.ShowAt(sender);
        }
    }
    
    #region AllowsClickThrough
    
    // private void AllowsClickThrough(FrameworkElement frameworkElement) {
    //     var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(MainWindow.AppWindow.Id);
    //     // textbox on titlebar area
    //     var transformTxtBox = frameworkElement.TransformToVisual(null);
    //     var bounds = transformTxtBox.TransformBounds(new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
    //     
    //     // Windows.Graphics.RectInt32[] rects defines the area which allows click throughs in custom titlebar
    //     // it is non dpi-aware client coordinates. Hence, we convert dpi aware coordinates to non-dpi coordinates
    //     // var scale = WindowHelper.GetRasterizationScaleForElement(MainWindow);
    //     
    //     var transparentRect = new Windows.Graphics.RectInt32 {
    //         X = (int)Math.Round(bounds.X),
    //         Y = (int)Math.Round(bounds.Y),
    //         Width = (int)Math.Round(bounds.Width),
    //         Height = (int)Math.Round(bounds.Height)
    //     };
    //     
    //     var rects = new[] { transparentRect };
    //     nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
    //     
    // }
    
    #endregion
    
    private async Task LoadNavigation() {
        var galleryService = Service.GetKeyedService<IGalleryService>(Settings.Source)!;
        var chapters = await galleryService.Chapters(Settings.SourcePath);
        var navigationItemModels = chapters.Select(CreateChapterNavigationItemModel);
        
        navigationView.MenuItems.Clear();
        await Navigator.AddNavigationMenuItems(navigationItemModels);
        
        // 默认导航到第一个菜单
        if (navigationView.MenuItems is { Count: > 0 } &&
            navigationView.MenuItems[0] is UIControls.NavigationViewItem navigationViewMenuItem) {
            Navigator.Navigate<Chapter>(navigationViewMenuItem);
        }
    }
    
    private void FrameInvoke(UIControls.Frame _) {
        frame.Navigated += (_, _) => { navigationView.IsBackEnabled = frame.CanGoBack; };
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
    
    private IEnumerable<NavigationItemModel<object>> CreateFooterNavigationItemModels() {
        gitInfoBadge = InfoBadge()
            .Value().Bind(vm?.CommitCount)
            .Visibility().Bind(vm?.CommitCount,
                convert: (int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed);
        return [
            new NavigationItemModel<object>() {
                Name = GitPage.Header,
                Content = GitPage.Header,
                Header = GitPage.Header,
                Page = typeof(GitPage),
                Icon = UIControls.Symbol.Remote,
                Payload = default,
                InfoBadge = gitInfoBadge
            }
        ];
    }
    
    private void NavigationInvoke(UIControls.NavigationView _) {
        Navigator.Init(navigationView, frame);
        
        navigationView.Loaded += async (sender, args) => {
            await Navigator.AddFooterNavigationMenuItems(CreateFooterNavigationItemModels());
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
                await Navigator.AddNavigationMenuItems(navigationItemModels, navigationViewItem);
            } else {
                // 补充其它的展开策略
            }
        };
        
        navigationView.BackRequested += (sender, args) => {
            if (frame is { CanGoBack: true }) {
                Back();
            } else {
                navigationView.IsBackEnabled = false;
            }
        };
        
        navigationView.ItemInvoked += (sender, args) => {
            // 防止重复点击
            if (args.InvokedItemContainer is UIControls.NavigationViewItem invokedItemContainer &&
                Navigator.IsCurrentPage(invokedItemContainer)) {
                return;
            }
            
            //设置导航写死了
            if (args.IsSettingsInvoked) {
                Navigator.Navigate(SettingPage.Header, typeof(SettingPage), SettingPage.Header,
                    args.RecommendedNavigationTransitionInfo);
            } else
                switch (args.InvokedItemContainer) {
                    case UIControls.NavigationViewItem { Tag: NavigationTag<Chapter> } chapterNavigationViewItem:
                        Navigator.Navigate<Chapter>(chapterNavigationViewItem,
                            args.RecommendedNavigationTransitionInfo);
                        break;
                    case UIControls.NavigationViewItem navigationViewItem:
                        Navigator.Navigate<object>(navigationViewItem, args.RecommendedNavigationTransitionInfo);
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
            Navigator.Back(pageStackEntry);
        }
    }
    
    private async void CreateAutoSyncCheck() {
        await vm?.Model.Status();
        var periodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) => { DispatcherQueue.TryEnqueue(async () => { await vm?.Model.Status(); }); },
            TimeSpan.FromSeconds(5));
    }
}

public partial record MainViewModel : BaseViewModel {
    public IState<int> SyncCount => State<int>.Empty(this);
    public IState<int> PushCount => State<int>.Empty(this);
    public IState<int> CommitCount => State<int>.Empty(this);
    public IListState<Branch> Branches => ListState<Branch>.Empty(this);
    public IState<string> BranchName => State<string>.Empty(this);
    public IListState<Commit> BeingPushedCommits => ListState<Commit>.Empty(this);
    public IState<string> CreateBranchName => State<string>.Value(this, () => string.Empty);
    
    public IState<bool> AddPullRequest => State<bool>.Value(this, () => true);
    
    public async Task GitPull() {
        try {
            if (string.IsNullOrWhiteSpace(Settings.LocalPath)) {
                InfoBarManager.Show(UIControls.InfoBarSeverity.Warning, GitPage.Header, "请先设置本地路径");
                return;
            }
            var gitApi = Service.GetService<IGitApi>()!;
            await Status();
            if (!gitApi.CheckWorkDir(Settings.LocalPath)) {
                await gitApi.Clone(Settings.GitRepos, Settings.LocalPath);
            } else {
                await gitApi.Pull(Settings.LocalPath, Settings.GitUserName);
            }
            
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success, GitPage.Header, "同步成功");
        } catch (Exception e) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, e.Message);
        }
    }
    
    public async Task GitPush() {
        try {
            var gitApi = Service.GetService<IGitApi>()!;
            var beingPushedCommits = await BeingPushedCommits;
            var commit = beingPushedCommits.FirstOrDefault();
            await gitApi.Push(Settings.LocalPath, Settings.GitUserName, Settings.GitAccessToken);
            var addPullRequest = await AddPullRequest;
            if (addPullRequest) {
                await PullRequest($"{commit?.Message}-{commit?.Author.When.ToString()}");
            }
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success, GitPage.Header, "推送成功");
        } catch (Exception e) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, e.Message);
        }
    }
    
    public async Task PullRequest(string message) {
        try {
            var gitApi = Service.GetService<IGitApi>()!;
            await gitApi.PullRequest(Settings.GitRepos, Settings.LocalPath, Settings.GitAccessToken, message, await BranchName);
            InfoBarManager.Show(UIControls.InfoBarSeverity.Success, GitPage.Header, "提交Pull Request成功");
        } catch (Exception e) {
            InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, e.Message);
        }
    }
    
    public async Task Status() {
        try {
            if (Settings.Source == Source.Local || Settings.LocalPath is null) {
                return;
            }
            var gitApi = Service.GetService<IGitApi>()!;
            var repositoryStatus = await gitApi.Status(Settings.LocalPath);
            if (repositoryStatus is null) {
                return;
            }
            
            await SyncCount.Update(_ => {
                var missingCount = repositoryStatus.Missing.Count();
                return missingCount;
            }, CancellationToken.None);
            
            await CommitCount.Update(_ => {
                var addCount = repositoryStatus.Added.Count();
                var modifyCount = repositoryStatus.Modified.Count();
                var removeCount = repositoryStatus.Removed.Count();
                return addCount + modifyCount + removeCount;
            }, CancellationToken.None);
            
            var commits = await gitApi.BeingPushedCommits(Settings.LocalPath);
            await PushCount.Update(_ => commits.Count(), CancellationToken.None);
        } catch (Exception e) {
            // InfoBarManager.Show(UIControls.InfoBarSeverity.Error, GitPage.Header, e.Message);
        }
    }
}
