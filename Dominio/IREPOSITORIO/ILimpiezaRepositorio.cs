using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface ILimpiezaRepositorio
    {
        Task<Limpieza> AgregarAsync(Limpieza limpieza);
        Task<Limpieza> ModificarAsync(Limpieza limpieza);
        Task<Limpieza?> GetOneAsync(int id);
        Task<IEnumerable<Limpieza>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<Limpieza>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<IEnumerable<Limpieza>> GetPendientesAsync();
        Task<IEnumerable<Limpieza>> GetFinalizadasAsync();
        Task<IEnumerable<Limpieza>> GetPorHabitacionAsync(int idHabitacion);
        Task<bool> IniciarAsync(int id);
        Task<bool> FinalizarAsync(int id, string? observacion = null);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
