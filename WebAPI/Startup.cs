using Application;
using Infrastructure.Interfaces.Services;
using Infrastructure.Services;

namespace WebAPI;

public static class Startup
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        services.AddServices();
        //services.AddPersistence(configuration);  ----> if we want to add SQLSERVER
        services.AddSingleton<IRedisService, RedisService>();
        services.AddSingleton<IEventBusService, EventBusService>();

        services.AddHostedService<EventBusListenerService>();
        services.AddSingleton<EventBusService>();
        services.AddApplication();
        return services;
    }
}