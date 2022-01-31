using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Termo.Infrastructure;

namespace Termo.API.Configurations
{
    public static class EntityFrameworkConfiguration
    {

        public static void AddEntityFrameworkConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseNpgsql(connectionString);
            }, ServiceLifetime.Scoped);
        }

    }
}
