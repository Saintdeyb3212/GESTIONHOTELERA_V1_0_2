using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface IClienteRepositorio
    {
        Task<Cliente> AgregarAsync(Cliente cliente);
        Task<Cliente> ModificarAsync(Cliente cliente);
        Task<Cliente?> GetOneAsync(int id);
        Task<Cliente?> GetPorDocumentoAsync(string numeroDocumento);
        Task<IEnumerable<Cliente>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<Cliente>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
