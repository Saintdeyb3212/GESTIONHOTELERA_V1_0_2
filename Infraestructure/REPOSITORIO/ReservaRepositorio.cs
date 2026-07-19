using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class ReservaRepositorio : IReservaRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReservaRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Reserva> AgregarAsync(Reserva reserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Reservas.AddAsync(reserva);
            await context.SaveChangesAsync();
            return reserva;
        }

        public async Task<IEnumerable<Reserva>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(r => r.Activo);
            return await query.OrderByDescending(r => r.FechaReserva).ToListAsync();
        }

        public async Task<Reserva?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).FirstOrDefaultAsync(r => r.IdReserva == id);
        }

        public async Task<Reserva> ModificarAsync(Reserva reserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == reserva.IdReserva);
            if (registro is null) return reserva;

            registro.FechaReserva = reserva.FechaReserva;
            registro.FechaCheckInProgramada = reserva.FechaCheckInProgramada;
            registro.FechaCheckOutProgramada = reserva.FechaCheckOutProgramada;
            registro.FechaCheckInReal = reserva.FechaCheckInReal;
            registro.FechaCheckOutReal = reserva.FechaCheckOutReal;
            registro.CantidadHuespedes = reserva.CantidadHuespedes;
            registro.EstadoReserva = reserva.EstadoReserva;
            registro.TotalEstimado = reserva.TotalEstimado;
            registro.Observaciones = reserva.Observaciones;
            registro.MotivoCancelacion = reserva.MotivoCancelacion;
            registro.FechaCancelacion = reserva.FechaCancelacion;
            registro.IdCliente = reserva.IdCliente;
            registro.Activo = reserva.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<Reserva>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(r => r.Activo);

            return await query
                .Where(r => r.EstadoReserva.Contains(criterio)
                    || r.Cliente.Nombres.Contains(criterio)
                    || r.Cliente.Apellidos.Contains(criterio)
                    || r.Cliente.NumeroDocumento.Contains(criterio))
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reserva>> GetPorEstadoAsync(string estadoReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context)
                .AsNoTracking()
                .Where(r => r.Activo && r.EstadoReserva == estadoReserva)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();
        }

        public Task<IEnumerable<Reserva>> GetPendientesAsync() => GetPorEstadoAsync(EstadosReserva.Pendiente);
        public Task<IEnumerable<Reserva>> GetConfirmadasAsync() => GetPorEstadoAsync(EstadosReserva.Confirmada);
        public Task<IEnumerable<Reserva>> GetCanceladasAsync() => GetPorEstadoAsync(EstadosReserva.Cancelada);
        public Task<IEnumerable<Reserva>> GetCompletadasAsync() => GetPorEstadoAsync(EstadosReserva.Completada);

        public async Task<bool> ConfirmarAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var reserva = await context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva is null) return false;
            reserva.EstadoReserva = EstadosReserva.Confirmada;
            reserva.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarCheckInAsync(int id, DateTime fechaCheckInReal)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var reserva = await context.Reservas.Include(r => r.DetallesReserva).FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva is null) return false;

            reserva.EstadoReserva = EstadosReserva.CheckIn;
            reserva.FechaCheckInReal = fechaCheckInReal;
            reserva.MarcarModificacion();

            var habitaciones = await context.Habitaciones
                .Where(h => reserva.DetallesReserva.Select(d => d.IdHabitacion).Contains(h.IdHabitacion))
                .ToListAsync();

            foreach (var habitacion in habitaciones)
            {
                habitacion.EstadoHabitacion = EstadosHabitacion.Ocupada;
                habitacion.MarcarModificacion();
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarCheckOutAsync(int id, DateTime fechaCheckOutReal)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var reserva = await context.Reservas.Include(r => r.DetallesReserva).FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva is null) return false;

            reserva.EstadoReserva = EstadosReserva.Completada;
            reserva.FechaCheckOutReal = fechaCheckOutReal;
            reserva.MarcarModificacion();

            var habitaciones = await context.Habitaciones
                .Where(h => reserva.DetallesReserva.Select(d => d.IdHabitacion).Contains(h.IdHabitacion))
                .ToListAsync();

            foreach (var habitacion in habitaciones)
            {
                habitacion.EstadoHabitacion = EstadosHabitacion.PendienteLimpieza;
                habitacion.MarcarModificacion();
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelarAsync(int id, string? motivoCancelacion = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var reserva = await context.Reservas.Include(r => r.DetallesReserva).FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reserva is null) return false;

            reserva.EstadoReserva = EstadosReserva.Cancelada;
            reserva.FechaCancelacion = DateTime.UtcNow;
            reserva.MotivoCancelacion = motivoCancelacion;
            reserva.MarcarModificacion();

            var habitaciones = await context.Habitaciones
                .Where(h => reserva.DetallesReserva.Select(d => d.IdHabitacion).Contains(h.IdHabitacion)
                    && h.EstadoHabitacion == EstadosHabitacion.Reservada)
                .ToListAsync();

            foreach (var habitacion in habitaciones)
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
            var registro = await context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<Reserva> QueryBase(ApplicationDbContext context)
        {
            return context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.DetallesReserva)
                    .ThenInclude(d => d.Habitacion)
                        .ThenInclude(h => h.TipoHabitacion)
                .Include(r => r.Pagos)
                    .ThenInclude(p => p.MetodoPago);
        }
    }

}
