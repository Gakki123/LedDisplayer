namespace LedDisplayer.Extensions;

/// <summary>
///     Extensions for <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" />
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Add StackExchange.Redis with its serialization provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="redisConfigurationFactory">The redis configration.</param>
    /// <typeparam name="T">The typof of serializer. <see cref="T:StackExchange.Redis.Extensions.Core.ISerializer" />.</typeparam>
    /*public static IServiceCollection AddStackExchangeRedisExtensions<T>(
        this IServiceCollection services,
        Func<IServiceProvider, IEnumerable<RedisConfiguration>> redisConfigurationFactory)
        where T : class, ISerializer
    {
        services.AddSingleton<IRedisClientFactory, RedisClientFactory>();
        services.AddSingleton<ISerializer, T>();
        services.AddSingleton<IRedisClient>(provider => provider.GetRequiredService<IRedisClientFactory>().GetDefaultRedisClient());
        services.AddSingleton<IRedisDatabase>(provider => provider.GetRequiredService<IRedisClientFactory>().GetDefaultRedisClient().GetDefaultDatabase());
        services.AddSingleton<IEnumerable<RedisConfiguration>>(redisConfigurationFactory);
        return services;
    }*/
}
