using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;


public interface IClienteServicio
{
    Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool incluirInactivos = false);
    Task<IEnumerable<Cliente>> BuscarAsync(string criterio, bool incluirInactivos = false);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento);
    Task<ResultadoOperacion<Cliente>> GuardarAsync(Cliente cliente);
    Task<ResultadoOperacion> ActivarAsync(int id);
    Task<ResultadoOperacion> DesactivarAsync(int id);
}
