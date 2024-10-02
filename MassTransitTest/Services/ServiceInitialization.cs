using MassTransit;
using MassTransitTest.BackgroudServices;
using MassTransitTest.BackgroudServices.Base;
using MassTransitTest.Data;
using MassTransitTest.Events;
using MassTransitTest.Sagas;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.Services
{
    public static class ServiceInitialization
    {
        public static void RegisterContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FileDatabaseContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure()));

            services.AddHostedService<MigrationHostedService<FileDatabaseContext>>();
        }

        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISendFileBackgroudService, SendFileBackgroudService>();
            services.AddHostedService<ScopedBackgroundService<ISendFileBackgroudService>>();
        }

        public static void RegisterMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<SqlTransportOptions>().Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
                //options.Host = "localhost";
                options.Database = "Masstransit";
                options.Schema = "transport"; // the schema for the transport-related tables, etc. 
            });

            services.AddSqlServerMigrationHostedService(x =>
            {
                x.CreateDatabase = false;
                x.CreateInfrastructure = true; // this is the default, but shown for completeness
            });

            services.AddMassTransit(x =>
            {
                x.AddSqlMessageScheduler();

                x.SetKebabCaseEndpointNameFormatter();

                x.AddEntityFrameworkOutbox<FileDatabaseContext>(o =>
                {
                    // configure which database lock provider to use (Postgres, SqlServer, or MySql)
                    o.UseSqlServer();

                    // enable the bus outbox
                    o.UseBusOutbox();
                });

                x.SetEntityFrameworkSagaRepositoryProvider(r =>
                {
                    r.ExistingDbContext<FileDatabaseContext>();
                    r.UseSqlServer();
                });

                x.AddConfigureEndpointsCallback((context, _, cfg) =>
                {
                    cfg.UseDelayedRedelivery(r =>
                    {
                        r.Handle<Exception>();
                        r.Interval(10000, 15000);
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Handle<Exception>();
                        r.Interval(25, 50);
                    });

                    cfg.UseEntityFrameworkOutbox<FileDatabaseContext>(context);
                });

                x.AddSagaStateMachine<FileStateMachine, FileState>()
                    .EntityFrameworkRepository(x =>
                    {
                        x.ExistingDbContext<FileDatabaseContext>();
                        x.UseSqlServer();
                    });

                x.AddConsumer<SendFileCommandConsummer>();

                x.AddConsumersFromNamespaceContaining<SendFileCommand>();
                x.AddActivitiesFromNamespaceContaining<SendFileCommand>();
                x.AddSagaStateMachinesFromNamespaceContaining<SendFileCommand>();

                x.UsingSqlServer((context, cfg) =>
                {
                    cfg.UseSqlMessageScheduler();

                    cfg.AutoStart = true;

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}
