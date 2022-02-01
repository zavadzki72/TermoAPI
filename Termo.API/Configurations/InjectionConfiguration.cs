using Microsoft.Extensions.DependencyInjection;
using Termo.API.Services;
using Termo.Infrastructure.Repositories;
using Termo.Models;
using Termo.Models.Interfaces;

namespace Termo.API.Configurations
{
    public static class InjectionConfiguration
    {

        public static void AddInjectionConfiguration(this IServiceCollection services)
        {
            services.AddServices();
            services.AddRepositories();
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IWorldService, WorldService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<ITermostatoService, TermostatoService>();
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IWorldRepository, WorldRepository>();
            services.AddScoped<ITryRepository, TryRepository>();
            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IInvalidWorldRepository, InvalidWorldRepository>();
        }
    }
}
