using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IDetalleReservaRepositorio
    {
        Task<DetalleReserva> AgregarAsync(DetalleReserva detalleReserva);
        Task<DetalleReserva> ModificarAsync(DetalleReserva detalleReserva);
        Task<DetalleReserva?> GetOneAsync(int id);
        Task<IEnumerable<DetalleReserva>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<DetalleReserva>> GetPorReservaAsync(int idReserva);
        Task<IEnumerable<DetalleReserva>> GetPorHabitacionAsync(int idHabitacion);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
