using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IMetodoPagoRepositorio
    {
        Task<MetodoPago> AgregarAsync(MetodoPago metodoPago);
        Task<MetodoPago> ModificarAsync(MetodoPago metodoPago);
        Task<MetodoPago?> GetOneAsync(int id);
        Task<IEnumerable<MetodoPago>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<MetodoPago>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }

}
