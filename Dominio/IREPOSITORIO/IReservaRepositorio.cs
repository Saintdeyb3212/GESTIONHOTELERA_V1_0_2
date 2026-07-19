using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IReservaRepositorio
    {
        Task<Reserva> AgregarAsync(Reserva reserva);
        Task<Reserva> ModificarAsync(Reserva reserva);
        Task<Reserva?> GetOneAsync(int id);
        Task<IEnumerable<Reserva>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<Reserva>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<IEnumerable<Reserva>> GetPorEstadoAsync(string estadoReserva);
        Task<IEnumerable<Reserva>> GetPendientesAsync();
        Task<IEnumerable<Reserva>> GetConfirmadasAsync();
        Task<IEnumerable<Reserva>> GetCanceladasAsync();
        Task<IEnumerable<Reserva>> GetCompletadasAsync();
        Task<bool> ConfirmarAsync(int id);
        Task<bool> RegistrarCheckInAsync(int id, DateTime fechaCheckInReal);
        Task<bool> RegistrarCheckOutAsync(int id, DateTime fechaCheckOutReal);
        Task<bool> CancelarAsync(int id, string? motivoCancelacion = null);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }

}
