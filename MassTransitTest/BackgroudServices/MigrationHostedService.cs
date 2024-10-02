using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.BackgroudServices;
public class MigrationHostedService<TDbContext> :IHostedService where TDbContext : DbContext
{
    readonly ILogger<MigrationHostedService<TDbContext>> _logger;
    readonly IServiceScopeFactory _scopeFactory;

    public MigrationHostedService(IServiceScopeFactory scopeFactory, ILogger<MigrationHostedService<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Applying migrations for {DbContext}", TypeCache<TDbContext>.ShortName);

        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
