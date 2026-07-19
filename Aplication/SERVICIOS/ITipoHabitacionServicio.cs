using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface ITipoHabitacionServicio
{
    Task<IEnumerable<TipoHabitacion>> ObtenerTodosAsync(bool incluirInactivos = false);
    Task<IEnumerable<TipoHabitacion>> BuscarAsync(string criterio, bool incluirInactivos = false);
    Task<TipoHabitacion?> ObtenerPorIdAsync(int id);
    Task<ResultadoOperacion<TipoHabitacion>> GuardarAsync(TipoHabitacion tipoHabitacion);
    Task<ResultadoOperacion> ActivarAsync(int id);
    Task<ResultadoOperacion> DesactivarAsync(int id);
}
