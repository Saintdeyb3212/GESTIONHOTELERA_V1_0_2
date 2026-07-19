using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IPagoRepositorio
    {
        Task<Pago> AgregarAsync(Pago pago);
        Task<Pago> ModificarAsync(Pago pago);
        Task<Pago?> GetOneAsync(int id);
        Task<IEnumerable<Pago>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<Pago>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<IEnumerable<Pago>> GetPorReservaAsync(int idReserva);
        Task<decimal> GetTotalPagadoReservaAsync(int idReserva);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
