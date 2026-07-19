using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IHabitacionRepositorio
    {
        Task<Habitacion> AgregarAsync(Habitacion habitacion);
        Task<Habitacion> ModificarAsync(Habitacion habitacion);
        Task<Habitacion?> GetOneAsync(int id);
        Task<IEnumerable<Habitacion>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<Habitacion>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<IEnumerable<Habitacion>> GetDisponiblesAsync(DateTime? checkIn = null, DateTime? checkOut = null, int? capacidadMinima = null);
        Task<IEnumerable<Habitacion>> GetPendientesLimpiezaAsync();
        Task<bool> CambiarEstadoAsync(int id, string estadoHabitacion);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
