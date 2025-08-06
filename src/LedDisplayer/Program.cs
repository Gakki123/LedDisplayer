using LedDisplayer.Services;
using Serilog;
using StackExchange.Redis;

namespace LedDisplayer;

internal static class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
            {
                Args = args,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
            });

            string? redisConn = builder.Configuration.GetConnectionString("Redis");

            if (string.IsNullOrWhiteSpace(redisConn))
            {
                throw new Exception("Redis connection string not configured");
            }

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));

            builder.Services.AddSingleton<LedManager>();
            builder.Services.AddSingleton<LedOperator>();
            builder.Services.AddHostedService<LedCheckService>();
            builder.Services.AddHostedService<LedDisplayService>();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

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
