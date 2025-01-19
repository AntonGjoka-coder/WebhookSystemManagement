using System.Reflection;
using AutoMapper;
using FluentValidation;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class Startup
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            return services.AddScoped(provider => new MapperConfiguration(cfg =>
                    {
                        cfg.AddMaps(executingAssembly);
                    })
                    .CreateMapper())
                .AddValidatorsFromAssembly(executingAssembly)
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(executingAssembly));
        }
        public static IServiceCollection AddServices(this IServiceCollection services) =>
            services
                .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
                .AddServices(typeof(ITransientService), ServiceLifetime.Singleton)
                .AddServices(typeof(IScopedService), ServiceLifetime.Scoped);
        
        public static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType,
            ServiceLifetime lifetime)
        {
            var interfaceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Service = t.GetInterfaces().FirstOrDefault(),
                    Implementation = t
                })
                .Where(t => t.Service is not null && interfaceType.IsAssignableFrom(t.Service));

            foreach (var type in interfaceTypes)
            {
                services.AddService(type.Service, type.Implementation, lifetime);
            }

            return services;
        }

        public static IServiceCollection AddService(this IServiceCollection services, Type serviceType,
            Type implementationType, ServiceLifetime lifetime) =>
            lifetime switch

            {
                ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
                ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
                ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
                _ => throw new ArgumentException("invalid lifetime", nameof(lifetime))
            };
    }
}
