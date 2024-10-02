namespace MassTransitTest.BackgroudServices.Base
{
    public interface IScopedProcessingService
    {
        Task DoWorkAsync(CancellationToken stoppingToken);

        Task StopAsync(CancellationToken stoppingToken);
    }

    public sealed class ScopedBackgroundService<T> : BackgroundService where T : IScopedProcessingService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private T _service;
        private IServiceScope _scope;

        public ScopedBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWorkAsync(stoppingToken);
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _scope = _serviceScopeFactory.CreateScope();

            _service = _scope.ServiceProvider.GetRequiredService<T>();

            await _service.DoWorkAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _service.StopAsync(stoppingToken);
            _scope.Dispose();
            await base.StopAsync(stoppingToken);
        }
    }
}
