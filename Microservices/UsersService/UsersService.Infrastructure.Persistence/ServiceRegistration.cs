namespace UsersService.Infrastructure.Persistence;

using UsersService.Application.Interfaces.Repositories;
using UsersService.Infrastructure.Persistence.Contexts;
using UsersService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistration
{
    public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        if (bool.Parse(configuration.GetSection("UseInMemoryDatabase").Value))
        {
            services.AddDbContext<UsersServiceDbContext>(options =>
                options.UseInMemoryDatabase("UsersServiceDb"));
        }
        else
        {

            services.AddDbContext<UsersServiceDbContext>(options =>
            options.UseNpgsql(
               configuration.GetConnectionString("DefaultConnection"),
               b => b.MigrationsAssembly(typeof(UsersServiceDbContext).Assembly.FullName)));
        }

        services.AddScoped(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
        services.AddScoped<IClaimRepositoryAsync, ClaimRepositoryAsync>();

    }
}
