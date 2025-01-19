using Application;
using Infrastructure.Services;

namespace WebAPI;

public static class Startup
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services,  IConfiguration configuration)
    {
        services.AddServices();
        //services.AddPersistence(configuration);  ----> if we want to add SQLSERVER
        services.AddSingleton(new RedisService(configuration.GetConnectionString("RedisConnection")));
        services.AddHostedService<EventBusListenerService>();
        services.AddSingleton<EventBusService>();
        services.AddApplication();
        return services;
    }
}