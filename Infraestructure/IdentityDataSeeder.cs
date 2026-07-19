using GESTIONHOTELERA_V1_0_2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace GESTIONHOTELERA_V1_0_2.Infraestructure
{
    public static class IdentityDataSeeder
    {
        private const string AdminEmail = "administrador@hotel.local";
        private const string EmpleadoEmail = "empleado@hotel.local";

        private const string AdminPassword = "Adm!2026#Hotel.Walay123";
        private const string EmpleadoPassword = "Emp!2026#Hotel.Walay123";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = new[] { "Administrador", "Empleado", "Recepcion", "Limpieza" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"No se pudo crear el rol '{role}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            await CreateUserIfMissingAsync(userManager, AdminEmail, AdminPassword, new[] { "Administrador" });
            await CreateUserIfMissingAsync(userManager, EmpleadoEmail, EmpleadoPassword, new[] { "Empleado", "Recepcion", "Limpieza" });
        }

        private static async Task CreateUserIfMissingAsync(UserManager<ApplicationUser> userManager, string email, string password, string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Activo = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException($"No se pudo crear el usuario '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            if (roles.Contains("Administrador") && !user.Activo)
            {
                user.Activo = true;
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException($"No se pudo activar el usuario '{email}': {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }
            }

            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, role);
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException($"No se pudo asignar el rol '{role}' al usuario '{email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
