using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class MetodoPagoRepositorio : IMetodoPagoRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public MetodoPagoRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<MetodoPago> AgregarAsync(MetodoPago metodoPago)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.MetodosPago.AddAsync(metodoPago);
            await context.SaveChangesAsync();
            return metodoPago;
        }

        public async Task<IEnumerable<MetodoPago>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.MetodosPago.AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(m => m.Activo);
            return await query.OrderBy(m => m.NombreMetodoPago).ToListAsync();
        }

        public async Task<MetodoPago?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.MetodosPago.Include(m => m.Pagos).FirstOrDefaultAsync(m => m.IdMetodoPago == id);
        }

        public async Task<MetodoPago> ModificarAsync(MetodoPago metodoPago)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.MetodosPago.FirstOrDefaultAsync(m => m.IdMetodoPago == metodoPago.IdMetodoPago);
            if (registro is null) return metodoPago;

            registro.NombreMetodoPago = metodoPago.NombreMetodoPago;
            registro.DescripcionMetodoPago = metodoPago.DescripcionMetodoPago;
            registro.Activo = metodoPago.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<MetodoPago>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = context.MetodosPago.AsNoTracking().AsQueryable();
            if (!incluirInactivos) query = query.Where(m => m.Activo);

            return await query
                .Where(m => m.NombreMetodoPago.Contains(criterio) || (m.DescripcionMetodoPago != null && m.DescripcionMetodoPago.Contains(criterio)))
                .OrderBy(m => m.NombreMetodoPago)
                .ToListAsync();
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.MetodosPago.FirstOrDefaultAsync(m => m.IdMetodoPago == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }
    }


}
