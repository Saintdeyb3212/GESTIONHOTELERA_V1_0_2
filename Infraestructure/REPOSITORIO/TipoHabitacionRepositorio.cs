using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class TipoHabitacionRepositorio : ITipoHabitacionRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public TipoHabitacionRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<TipoHabitacion> AgregarAsync(TipoHabitacion tipoHabitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.TipoHabitaciones.AddAsync(tipoHabitacion);
            await context.SaveChangesAsync();
            return tipoHabitacion;
        }

        public async Task<IEnumerable<TipoHabitacion>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.TipoHabitaciones.AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(t => t.Activo);
            return await query.OrderBy(t => t.NombreTipoHabitacion).ToListAsync();
        }

        public async Task<TipoHabitacion?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.TipoHabitaciones
                .Include(t => t.Habitaciones)
                .FirstOrDefaultAsync(t => t.IdTipoHabitacion == id);
        }

        public async Task<TipoHabitacion> ModificarAsync(TipoHabitacion tipoHabitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.TipoHabitaciones.FirstOrDefaultAsync(t => t.IdTipoHabitacion == tipoHabitacion.IdTipoHabitacion);
            if (registro is null) return tipoHabitacion;

            registro.NombreTipoHabitacion = tipoHabitacion.NombreTipoHabitacion;
            registro.DescripcionTipoHabitacion = tipoHabitacion.DescripcionTipoHabitacion;
            registro.CapacidadPersonas = tipoHabitacion.CapacidadPersonas;
            registro.PrecioPorNoche = tipoHabitacion.PrecioPorNoche;
            registro.Activo = tipoHabitacion.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<TipoHabitacion>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = context.TipoHabitaciones.AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(t => t.Activo);

            return await query
                .Where(t => t.NombreTipoHabitacion.Contains(criterio) || (t.DescripcionTipoHabitacion != null && t.DescripcionTipoHabitacion.Contains(criterio)))
                .OrderBy(t => t.NombreTipoHabitacion)
                .ToListAsync();
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.TipoHabitaciones.FirstOrDefaultAsync(t => t.IdTipoHabitacion == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }
    }
}
