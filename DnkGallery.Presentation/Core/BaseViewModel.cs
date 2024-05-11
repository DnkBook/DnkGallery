namespace DnkGallery.Presentation.Core;

public record BaseViewModel {
    protected IState<T> UseState<T>(Func<T> valueProvider) => State<T>.Value(this, valueProvider);
    protected static ValueTask SetState<T>(IState<T> state,Func<T?,T?> updater) => state.Update(updater, CancellationToken.None);
}
