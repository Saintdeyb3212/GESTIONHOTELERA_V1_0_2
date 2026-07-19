using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class LimpiezaRepositorio : ILimpiezaRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public LimpiezaRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Limpieza> AgregarAsync(Limpieza limpieza)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Limpiezas.AddAsync(limpieza);

            var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == limpieza.IdHabitacion);
            if (habitacion is not null)
            {
                habitacion.EstadoHabitacion = EstadosHabitacion.EnLimpieza;
                habitacion.MarcarModificacion();
            }

            await context.SaveChangesAsync();
            return limpieza;
        }

        public async Task<IEnumerable<Limpieza>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(l => l.Activo);
            return await query.OrderByDescending(l => l.FechaAsignacion).ToListAsync();
        }

        public async Task<Limpieza?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).FirstOrDefaultAsync(l => l.IdLimpieza == id);
        }

        public async Task<Limpieza> ModificarAsync(Limpieza limpieza)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == limpieza.IdLimpieza);
            if (registro is null) return limpieza;

            registro.FechaAsignacion = limpieza.FechaAsignacion;
            registro.FechaInicio = limpieza.FechaInicio;
            registro.FechaFinalizacion = limpieza.FechaFinalizacion;
            registro.ResponsableLimpieza = limpieza.ResponsableLimpieza;
            registro.EstadoLimpieza = limpieza.EstadoLimpieza;
            registro.Observacion = limpieza.Observacion;
            registro.IdHabitacion = limpieza.IdHabitacion;
            registro.Activo = limpieza.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<Limpieza>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(l => l.Activo);

            return await query
                .Where(l => l.ResponsableLimpieza.Contains(criterio)
                    || l.EstadoLimpieza.Contains(criterio)
                    || l.Habitacion.NumeroHabitacion.Contains(criterio)
                    || (l.Observacion != null && l.Observacion.Contains(criterio)))
                .OrderByDescending(l => l.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Limpieza>> GetPendientesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking()
                .Where(l => l.Activo && l.EstadoLimpieza == EstadosLimpieza.Pendiente)
                .OrderBy(l => l.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Limpieza>> GetFinalizadasAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking()
                .Where(l => l.Activo && l.EstadoLimpieza == EstadosLimpieza.Finalizada)
                .OrderByDescending(l => l.FechaFinalizacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Limpieza>> GetPorHabitacionAsync(int idHabitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking()
                .Where(l => l.Activo && l.IdHabitacion == idHabitacion)
                .OrderByDescending(l => l.FechaAsignacion)
                .ToListAsync();
        }

        public async Task<bool> IniciarAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var limpieza = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == id);
            if (limpieza is null) return false;
            limpieza.EstadoLimpieza = EstadosLimpieza.EnProceso;
            limpieza.FechaInicio = DateTime.UtcNow;
            limpieza.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FinalizarAsync(int id, string? observacion = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var limpieza = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == id);
            if (limpieza is null) return false;

            limpieza.EstadoLimpieza = EstadosLimpieza.Finalizada;
            limpieza.FechaFinalizacion = DateTime.UtcNow;
            limpieza.Observacion = observacion ?? limpieza.Observacion;
            limpieza.MarcarModificacion();

            var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == limpieza.IdHabitacion);
            if (habitacion is not null)
            {
                habitacion.EstadoHabitacion = EstadosHabitacion.Disponible;
                habitacion.MarcarModificacion();
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Limpiezas.FirstOrDefaultAsync(l => l.IdLimpieza == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<Limpieza> QueryBase(ApplicationDbContext context)
        {
            return context.Limpiezas
                .Include(l => l.Habitacion)
                    .ThenInclude(h => h.TipoHabitacion);
        }
    }
}
