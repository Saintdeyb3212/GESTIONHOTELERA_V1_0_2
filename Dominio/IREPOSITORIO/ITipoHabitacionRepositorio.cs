using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO
{
    public interface ITipoHabitacionRepositorio
    {
        Task<TipoHabitacion> AgregarAsync(TipoHabitacion tipoHabitacion);
        Task<TipoHabitacion> ModificarAsync(TipoHabitacion tipoHabitacion);
        Task<TipoHabitacion?> GetOneAsync(int id);
        Task<IEnumerable<TipoHabitacion>> GetAllAsync(bool incluirInactivos = false);
        Task<IEnumerable<TipoHabitacion>> BuscarAsync(string criterio, bool incluirInactivos = false);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);
    }
}
