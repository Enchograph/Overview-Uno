namespace Overview.Client.Infrastructure.Widgets;

public interface IWidgetRenderer
{
    Task RenderAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken = default);

    Task ClearAsync(WidgetKind kind, CancellationToken cancellationToken = default);
}

public sealed class NoOpWidgetRenderer : IWidgetRenderer
{
    public Task RenderAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ClearAsync(WidgetKind kind, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

public sealed class PlatformWidgetRenderer : IWidgetRenderer
{
#if __ANDROID__
    private readonly IWidgetRenderer innerRenderer = new Overview.Client.Droid.Widgets.AndroidWidgetRenderer();
#else
    private readonly IWidgetRenderer innerRenderer = new NoOpWidgetRenderer();
#endif

    public Task RenderAsync(WidgetSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        return innerRenderer.RenderAsync(snapshot, cancellationToken);
    }

    public Task ClearAsync(WidgetKind kind, CancellationToken cancellationToken = default)
    {
        return innerRenderer.ClearAsync(kind, cancellationToken);
    }
}
