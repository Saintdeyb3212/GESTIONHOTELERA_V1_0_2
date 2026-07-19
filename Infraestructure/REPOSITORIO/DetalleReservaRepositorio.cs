using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class DetalleReservaRepositorio : IDetalleReservaRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public DetalleReservaRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<DetalleReserva> AgregarAsync(DetalleReserva detalleReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.DetallesReserva.AddAsync(detalleReserva);
            await context.SaveChangesAsync();
            return detalleReserva;
        }

        public async Task<IEnumerable<DetalleReserva>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(d => d.Activo);
            return await query.OrderByDescending(d => d.IdDetalleReserva).ToListAsync();
        }

        public async Task<DetalleReserva?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).FirstOrDefaultAsync(d => d.IdDetalleReserva == id);
        }

        public async Task<DetalleReserva> ModificarAsync(DetalleReserva detalleReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.DetallesReserva.FirstOrDefaultAsync(d => d.IdDetalleReserva == detalleReserva.IdDetalleReserva);
            if (registro is null) return detalleReserva;

            registro.IdReserva = detalleReserva.IdReserva;
            registro.IdHabitacion = detalleReserva.IdHabitacion;
            registro.PrecioPorNoche = detalleReserva.PrecioPorNoche;
            registro.CantidadNoches = detalleReserva.CantidadNoches;
            registro.SubTotal = detalleReserva.SubTotal;
            registro.Activo = detalleReserva.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<DetalleReserva>> GetPorReservaAsync(int idReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking().Where(d => d.Activo && d.IdReserva == idReserva).ToListAsync();
        }

        public async Task<IEnumerable<DetalleReserva>> GetPorHabitacionAsync(int idHabitacion)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking().Where(d => d.Activo && d.IdHabitacion == idHabitacion).ToListAsync();
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.DetallesReserva.FirstOrDefaultAsync(d => d.IdDetalleReserva == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<DetalleReserva> QueryBase(ApplicationDbContext context)
        {
            return context.DetallesReserva
                .Include(d => d.Reserva)
                    .ThenInclude(r => r.Cliente)
                .Include(d => d.Habitacion)
                    .ThenInclude(h => h.TipoHabitacion);
        }
    }
}
