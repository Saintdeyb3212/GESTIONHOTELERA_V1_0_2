using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class HabitacionRepositorio : IHabitacionRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public HabitacionRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Habitacion> AgregarAsync(Habitacion habitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Habitaciones.AddAsync(habitacion);
            await context.SaveChangesAsync();
            return habitacion;
        }

        public async Task<IEnumerable<Habitacion>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Habitaciones.Include(h => h.TipoHabitacion).AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(h => h.Activo);
            return await query.OrderBy(h => h.NumeroHabitacion).ToListAsync();
        }

        public async Task<Habitacion?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Habitaciones
                .Include(h => h.TipoHabitacion)
                .Include(h => h.Limpiezas)
                .FirstOrDefaultAsync(h => h.IdHabitacion == id);
        }

        public async Task<Habitacion> ModificarAsync(Habitacion habitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == habitacion.IdHabitacion);
            if (registro is null) return habitacion;

            registro.NumeroHabitacion = habitacion.NumeroHabitacion;
            registro.Piso = habitacion.Piso;
            registro.DescripcionHabitacion = habitacion.DescripcionHabitacion;
            registro.EstadoHabitacion = habitacion.EstadoHabitacion;
            registro.IdTipoHabitacion = habitacion.IdTipoHabitacion;
            registro.Activo = habitacion.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<Habitacion>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = context.Habitaciones.Include(h => h.TipoHabitacion).AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(h => h.Activo);

            return await query
                .Where(h => h.NumeroHabitacion.Contains(criterio)
                    || h.EstadoHabitacion.Contains(criterio)
                    || (h.Piso != null && h.Piso.Contains(criterio))
                    || h.TipoHabitacion.NombreTipoHabitacion.Contains(criterio))
                .OrderBy(h => h.NumeroHabitacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Habitacion>> GetDisponiblesAsync(DateTime? checkIn = null, DateTime? checkOut = null, int? capacidadMinima = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Habitaciones
                .Include(h => h.TipoHabitacion)
                .AsNoTracking()
                .Where(h => h.Activo && h.EstadoHabitacion == EstadosHabitacion.Disponible);

            if (capacidadMinima.HasValue)
            {
                query = query.Where(h => h.TipoHabitacion.CapacidadPersonas >= capacidadMinima.Value);
            }

            if (checkIn.HasValue && checkOut.HasValue)
            {
                query = query.Where(h => !context.DetallesReserva.Any(d =>
                    d.IdHabitacion == h.IdHabitacion
                    && d.Activo
                    && d.Reserva.Activo
                    && d.Reserva.EstadoReserva != EstadosReserva.Cancelada
                    && checkIn.Value < d.Reserva.FechaCheckOutProgramada
                    && checkOut.Value > d.Reserva.FechaCheckInProgramada));
            }

            return await query.OrderBy(h => h.NumeroHabitacion).ToListAsync();
        }

        public async Task<IEnumerable<Habitacion>> GetPendientesLimpiezaAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Habitaciones
                .Include(h => h.TipoHabitacion)
                .AsNoTracking()
                .Where(h => h.Activo && h.EstadoHabitacion == EstadosHabitacion.PendienteLimpieza)
                .OrderBy(h => h.NumeroHabitacion)
                .ToListAsync();
        }

        public async Task<bool> CambiarEstadoAsync(int id, string estadoHabitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == id);
            if (registro is null) return false;
            registro.EstadoHabitacion = estadoHabitacion;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.EstadoHabitacion = activo ? EstadosHabitacion.Disponible : EstadosHabitacion.Inactiva;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }
    }
}
