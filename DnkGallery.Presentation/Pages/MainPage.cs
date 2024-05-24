using LibGit2Sharp;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using DataTemplate = CSharpMarkup.WinUI.DataTemplate;

namespace DnkGallery.Presentation.Pages;

partial class MainPage {
    private Grid AppTitleBar(string title) => Grid(Columns(Auto, Auto, Auto),
            FontIcon(new FontFamily("Segoe UI Emoji")).Glyph("🥵")
                .HorizontalAlignment(HorizontalAlignment.Left)
                .VCenter(),
            TextBlock()
                .Margin(12, 0, 0, 0)
                .Grid_Column(1)
                .Text(title)
                .VCenter().Margin(28, 0, 0, 0)
        )
        .Height(48)
        .Margin(48, 0, 0, 0)
        .VerticalAlignment(VerticalAlignment.Top)
        .Padding(0);
    
    public void BuildUI() => Content(
        Grid(AppTitleBar("DnkGallery"),
            NavigationView(
                    Frame().Assign(out frame).Invoke(FrameInvoke))
                .PaneHeader(
                    HStack(
                        Grid(
                            HyperlinkButton(
                                    FontIcon(FontSize: 14).Glyph("\uE895").HCenter().VCenter())
                                .ToolTipService_ToolTip("同步").Height(36).BindCommand(vm?.GitPull),
                            InfoBadge()
                                .Value().Bind(vm?.SyncCount)
                                .Visibility().Bind(vm?.SyncCount,
                                    convert: (int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed)
                                .HorizontalAlignment(HorizontalAlignment.Right)
                                .VerticalAlignment(VerticalAlignment.Top)
                        ),
                        Grid(
                            HyperlinkButton(FontIcon(FontSize: 14).Glyph("\uE8AD"))
                                .Height(36).Assign(out pushHyperlinkButton)
                                .ToolTipService_ToolTip("推送"),
                            InfoBadge()
                                .Value().Bind(vm?.PushCount)
                                .Visibility().Bind(vm?.PushCount,
                                    convert: (int count) => count > 0 ? Visibility.Visible : Visibility.Collapsed)
                                .HorizontalAlignment(HorizontalAlignment.Right)
                                .VerticalAlignment(VerticalAlignment.Top)
                        ),
                        Grid(
                            ComboBox()
                                .IsEditable(true)
                                .Assign(out branchesComboBox)
                                .SelectedValue().Bind(vm?.BranchName,BindingMode.TwoWay)
                                .Width(160)
                                .DisplayMemberPath("FriendlyName")
                                .SelectedValuePath("FriendlyName")
                                .ItemsSource().Bind(vm?.Branches)
                                .ContextFlyout(Flyout(
                                    VStack(
                                        TextBlock().Bind(vm?.CreateBranchName,
                                                convert: (string name) => $"是否创建{name}分支"),
                                        HStack(
                                            Button("创建").Assign(out branchesComboBoxFlyoutConfirm)
                                                .Style(ThemeResource.AccentButtonStyle),
                                            Button("取消").Assign(out branchesComboBoxFlyoutCancel)
                                        ).Spacing(16)
                                    ).Spacing(16)
                                )).Margin(4, 0, 0, 0).HCenter().VCenter()
                        ).Height(36)
                    ).Height(44).Margin(16, 0)
                )
                .IsBackEnabled(true)
                .Assign(out navigationView)
                .Invoke(NavigationInvoke),
            // .MenuItemsSource()
            // .Bind(vm?.MenuItems)
            // .MenuItemTemplate(MenuItemTemplate)
            ContentDialog(
                    Grid(
                        ListView(() =>
                            VStack(
                                TextBlock().Bind("MessageShort")
                                    .FontSize(16)
                                    .FontWeight(FontWeights.Bold),
                                TextBlock().Bind("Committer")
                                    .FontSize(12),
                                TextBlock()
                                    .Bind("Committer",
                                        convert: (Signature signature) =>
                                            signature.When.ToString("yyyy-MM-dd HH:mm:ss"))
                                    .FontSize(12)
                            ).Padding(8)
                        ).ItemsSource().Bind(vm?.BeingPushedCommits)
                    )
                )
                .XamlRoot(XamlRoot)
                .Title(GitPage.Header)
                .PrimaryButtonText("推送")
                .PrimaryButtonCommand().Bind(vm?.GitPush)
                .DefaultButton(UIControls.ContentDialogButton.Primary)
                .SecondaryButtonText("取消")
                .Assign(out pushDialog)
        ).Invoke(GridInvoke)
    );
    
    
    private DataTemplate MenuItemTemplate => DataTemplate(
        () => NavigationViewItem()
            .Content().Bind("Name")
            .Icon().Bind("Icon")
            .MenuItemsSource().Bind("Children")
            .Tag().Bind("Name")
    );
}
