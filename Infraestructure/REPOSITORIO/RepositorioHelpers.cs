using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    internal static class RepositorioHelpers
    {
        internal static void MarcarModificacion(this EntidadBase entidad)
        {
            entidad.FechaModificacion = DateTime.UtcNow;
        }
    }

}
