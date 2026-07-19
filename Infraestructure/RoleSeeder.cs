using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GESTIONHOTELERA_V1_0_2.Infraestructure
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = new[] { "Administrador", "Empleado", "Recepcion", "Limpieza" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}