using KellermanSoftware.CompareNetObjects;
using Serilog;
using StackExchange.Redis;

namespace LedDisplayer;

internal static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] ┊ {Message:lj}{NewLine}{Exception}",
                applyThemeToRedirectedOutput: true,
                syncRoot: Contracts.SyncRoot)
            .CreateLogger();

        try
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
            {
                Args = args,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            string? redisConn = builder.Configuration.GetConnectionString("Redis");

            if (string.IsNullOrWhiteSpace(redisConn))
            {
                throw new Exception("Redis connection string not configured");
            }

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                /*options.SerializerOptions.Converters.Add(new DateTimeConverter());
                options.SerializerOptions.Converters.Add(new DateTimeNullableConverter());
                options.SerializerOptions.Converters.Add(new DoubleConverter());*/
            });

            builder.Services.AddSingleton<ICompareLogic>(new CompareLogic(new ComparisonConfig()
            {
                MaxDifferences = 3
            }));

            builder.Services.AddSingleton<LedManager>();
            builder.Services.AddSingleton<LedOperator>();
            builder.Services.AddSingleton<LedCheckService>();
            builder.Services.AddSingleton<LedDisplayService>();

            builder.Services.AddHostedService<LedHostedService>();

            IHost host = builder.Build();
            host.Run();
        }
        catch (Exception e)
        {
            Log.Logger.Fatal("Stopped", e.Message);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
