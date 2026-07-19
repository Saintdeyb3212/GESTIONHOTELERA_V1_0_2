using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface IHabitacionServicio
{
    Task<IEnumerable<Habitacion>> ObtenerTodasAsync(bool incluirInactivos = false);
    Task<IEnumerable<Habitacion>> BuscarAsync(string criterio, bool incluirInactivos = false);
    Task<Habitacion?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<Habitacion>> GuardarAsync(Habitacion habitacion);
    Task<ResultadoOperacion> ActivarAsync(int id);
    Task<ResultadoOperacion> DesactivarAsync(int id);
    Task<ResultadoOperacion> CambiarEstadoAsync(int id, string estadoHabitacion);
}
