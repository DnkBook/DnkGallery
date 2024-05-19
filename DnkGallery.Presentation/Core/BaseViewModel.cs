using DnkGallery.Model;
using Microsoft.Extensions.DependencyInjection;
namespace DnkGallery.Presentation.Core;

public record BaseViewModel {
    protected IServiceProvider Service => Ioc.Service;
    protected Setting Settings => Ioc.Service.GetService<Setting>()!;
    protected IState<T> UseState<T>(Func<T> valueProvider) => State<T>.Value(this, valueProvider);
    protected static ValueTask SetState<T>(IState<T> state,Func<T?,T?> updater) => state.Update(updater, CancellationToken.None);
}
