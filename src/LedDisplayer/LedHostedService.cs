namespace LedDisplayer;

internal class LedHostedService : IHostedService, IDisposable
{
    private readonly LedCheckService _ledCheckService;
    private readonly LedDisplayService _ledDisplayService;
    private readonly ILogger<LedHostedService> _logger;

    private Timer? _ledCheckTimer;
    private Timer? _ledDisplayTimer;

    public LedHostedService(LedCheckService ledCheckService, LedDisplayService ledDisplayService, ILogger<LedHostedService> logger)
    {
        _ledCheckService = ledCheckService;
        _ledDisplayService = ledDisplayService;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ledCheckTimer = new Timer(_ledCheckService.DoWork, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5));
        _ledDisplayTimer = new Timer(_ledDisplayService.DoWork, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1));

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _ledCheckTimer?.Change(Timeout.Infinite, 0);
        _ledDisplayTimer?.Change(Timeout.Infinite, 0);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _ledCheckTimer?.Dispose();
        _ledDisplayTimer?.Dispose();
    }
}
