using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using System;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class PagoRepositorio : IPagoRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public PagoRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Pago> AgregarAsync(Pago pago)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Pagos.AddAsync(pago);
            await context.SaveChangesAsync();
            return pago;
        }

        public async Task<IEnumerable<Pago>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(p => p.Activo);
            return await query.OrderByDescending(p => p.FechaPago).ToListAsync();
        }

        public async Task<Pago?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).FirstOrDefaultAsync(p => p.IdPago == id);
        }

        public async Task<Pago> ModificarAsync(Pago pago)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Pagos.FirstOrDefaultAsync(p => p.IdPago == pago.IdPago);
            if (registro is null) return pago;

            registro.FechaPago = pago.FechaPago;
            registro.Monto = pago.Monto;
            registro.TipoPago = pago.TipoPago;
            registro.NumeroOperacion = pago.NumeroOperacion;
            registro.Observacion = pago.Observacion;
            registro.IdReserva = pago.IdReserva;
            registro.IdMetodoPago = pago.IdMetodoPago;
            registro.Activo = pago.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<Pago>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            criterio = criterio.Trim();
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = QueryBase(context).AsNoTracking();
            if (!incluirInactivos) query = query.Where(p => p.Activo);

            return await query
                .Where(p => p.TipoPago.Contains(criterio)
                    || p.MetodoPago.NombreMetodoPago.Contains(criterio)
                    || (p.NumeroOperacion != null && p.NumeroOperacion.Contains(criterio))
                    || p.Reserva.Cliente.Nombres.Contains(criterio)
                    || p.Reserva.Cliente.Apellidos.Contains(criterio))
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pago>> GetPorReservaAsync(int idReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await QueryBase(context).AsNoTracking()
                .Where(p => p.Activo && p.IdReserva == idReserva)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPagadoReservaAsync(int idReserva)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Pagos.AsNoTracking()
                .Where(p => p.Activo && p.IdReserva == idReserva)
                .SumAsync(p => p.Monto);
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Pagos.FirstOrDefaultAsync(p => p.IdPago == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<Pago> QueryBase(ApplicationDbContext context)
        {
            return context.Pagos
                .Include(p => p.Reserva)
                    .ThenInclude(r => r.Cliente)
                .Include(p => p.MetodoPago);
        }
    }
}
