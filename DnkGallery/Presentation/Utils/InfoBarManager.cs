using System.Numerics;
using Windows.System.Threading;
using Uno.Disposables;
namespace DnkGallery.Presentation.Utils;

public class InfoBarManager {
    private static Queue<UIControls.InfoBar> _infoBarQueue = new();
    private static int _maxCount;
    private static UIControls.Grid? _root;
    private static UIControls.StackPanel _showPanel;
    private static UIXaml.Window _mainWindow;
    private static UI.Composition.Compositor _compositor;
    public static void Init(UIXaml.Window mainWindow, UIControls.Grid root, int maxCount = 3) {
        _root = root;
        _maxCount = maxCount;
        _mainWindow = mainWindow;
        _compositor = _mainWindow.Compositor;
        _root.Children.Add(CreatePanel());
    }
    
    private static UIControls.StackPanel CreatePanel() {
        var stackPanel = VStack()
            .VerticalAlignment(VerticalAlignment.Top).Margin(0, 48, 0, 0)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .MaxWidth(500)
            .Spacing(8)
            .UI;
        _showPanel = stackPanel;
        return stackPanel;
    }
    private static UIControls.InfoBar Create(UIControls.InfoBarSeverity type, string title, string message) {
        if (_infoBarQueue.Count >= _maxCount && _infoBarQueue.TryDequeue(out var oldInfoBar)) {
            oldInfoBar.Severity = type;
            oldInfoBar.Title = title;
            oldInfoBar.Message = message;
            (oldInfoBar.Tag as ThreadPoolTimer)?.Cancel();
            _infoBarQueue.Enqueue(oldInfoBar);
            return oldInfoBar;
        }
        var infoBar = InfoBar()
            .Severity(type).Title(title).Message(message).Name(DateTime.Now.Ticks.ToString())
            .UI;
        infoBar.PointerEntered += InfoBarPointerEntered;
        infoBar.PointerExited += InfoBarPointerExited;
        _infoBarQueue.Enqueue(infoBar);
        return infoBar;
    }
    
    public static void Show(UIControls.InfoBarSeverity type, string title, string message, int autoCloseDelaySeconds = 5) {
        _root?.DispatcherQueue.TryEnqueue(() => {
            // 创建通知
            var infoBar = Create(type, title, message);
            // 创建自动关闭任务
            var autoCloseSchedule = CreateAutoCloseSchedule(infoBar, TimeSpan.FromSeconds(autoCloseDelaySeconds));
            // 设置到tag里
            infoBar.Tag = autoCloseSchedule;
            // 主动关闭关闭定时器
            infoBar.Closed += (sender, args) => {
                autoCloseSchedule.Cancel();
                autoCloseSchedule.TryDispose();
            };
            infoBar.PointerEntered += (sender, args) => {
                autoCloseSchedule.Cancel();
                autoCloseSchedule.TryDispose();
            };
            infoBar.PointerExited += (sender, args) => {
                autoCloseSchedule = CreateAutoCloseSchedule(infoBar, TimeSpan.FromSeconds(autoCloseDelaySeconds));
            };
            
            if (_showPanel.FindName(infoBar.Name) is not null) {
                _showPanel.Children.Remove(infoBar);
            }
            // 加入到ui
            _showPanel.Children.Add(infoBar);
            // 显示
            infoBar.IsOpen = true;
            InfoBarFadeIn(infoBar);
            InfoBarTranslationIn(infoBar);
        });
    }
    
    private static ThreadPoolTimer CreateAutoCloseSchedule(UIControls.InfoBar infoBar, TimeSpan delay) {
        var threadPoolTimer = ThreadPoolTimer.CreateTimer(_ => {
            _showPanel?.DispatcherQueue.TryEnqueue(async () => {
                InfoBarFadeOut(infoBar);
                await Task.Delay(FadePeriod);
                InfoBarTranslationOut(infoBar);
                // 多加100毫秒等待动画完成完全
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                infoBar.IsOpen = false;
            });
        }, delay);
        return threadPoolTimer;
    }
    
    
    private static UI.Composition.SpringVector3NaturalMotionAnimation? _scaleAnimation;
    private static UI.Composition.SpringScalarNaturalMotionAnimation _fadeAnimation;
    
    private static readonly TimeSpan FadePeriod = TimeSpan.FromMilliseconds(200);
    
    
    private static void CreateScaleAnimation(Vector3 initialValue, Vector3 finalValue) {
        if (_scaleAnimation is null) {
            _scaleAnimation = _compositor.CreateSpringVector3Animation();
            _scaleAnimation.Target = "Scale";
        }
        _scaleAnimation.InitialValue = initialValue;
        _scaleAnimation.FinalValue = finalValue;
    }
    
    private static void CreateFadeAnimation(float initialValue, float finalValue) {
        if (_fadeAnimation is null) {
            _fadeAnimation = _compositor.CreateSpringScalarAnimation();
            _fadeAnimation.Target = "Opacity";
        }
        _fadeAnimation.Period = FadePeriod;
        _fadeAnimation.InitialValue = initialValue;
        _fadeAnimation.FinalValue = finalValue;
    }
    
    private static void InfoBarPointerEntered(object sender, UIXaml.Input.PointerRoutedEventArgs e) {
        CreateScaleAnimation(new Vector3(1.0f), new Vector3(0.99f));
        (sender as UIXaml.UIElement)?.StartAnimation(_scaleAnimation);
    }
    
    private static void InfoBarPointerExited(object sender, UIXaml.Input.PointerRoutedEventArgs e) {
        CreateScaleAnimation(new Vector3(1.0f), new Vector3(0.99f));
        (sender as UIXaml.UIElement)?.StartAnimation(_scaleAnimation);
    }
    
    private static void InfoBarFadeIn(object sender) {
        CreateFadeAnimation(0, 1);
        (sender as UIXaml.UIElement)?.StartAnimation(_fadeAnimation);
    }
    
    private static void InfoBarFadeOut(object sender) {
        CreateFadeAnimation(1, 0);
        (sender as UIXaml.UIElement)?.StartAnimation(_fadeAnimation);
        
    }
    
    private static void InfoBarTranslationIn(object sender) {
        var uiElement = sender as UIControls.InfoBar;
        CreateScaleAnimation(new Vector3(0f), new Vector3(1f));
        uiElement?.StartAnimation(_scaleAnimation);
    }
    private static void InfoBarTranslationOut(object sender) {
        var uiElement = sender as UIControls.InfoBar;
        CreateScaleAnimation(new Vector3(1f), new Vector3(0f));
        uiElement?.StartAnimation(_scaleAnimation);
    }
}
