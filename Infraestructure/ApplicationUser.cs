using Microsoft.AspNetCore.Identity;

namespace GESTIONHOTELERA_V1_0_2.Data
{
    // Usuario de la aplicación. Activo controla si puede ingresar al sistema.
    public class ApplicationUser : IdentityUser
    {
        public bool Activo { get; set; } = true;
    }
}
