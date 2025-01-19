using Hangfire;
using Infrastructure.Interfaces.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default"), 
                    b => b.MigrationsAssembly("Infrastructure")));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());
            services.AddHangfire(config => config.UseSqlServerStorage(configuration.GetConnectionString("Default")));
            services.AddHangfireServer();

            
            return services;
        }
    }
}
