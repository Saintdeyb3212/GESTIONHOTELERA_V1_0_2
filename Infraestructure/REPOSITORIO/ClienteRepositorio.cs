using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO
{
    public class ClienteRepositorio : IClienteRepositorio
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ClienteRepositorio(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Cliente> AgregarAsync(Cliente cliente)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Clientes.AddAsync(cliente);
            await context.SaveChangesAsync();
            return cliente;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync(bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Clientes.AsNoTracking().Include(c => c.Reservas).AsQueryable();
            if (!incluirInactivos) query = query.Where(c => c.Activo);
            return await query.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();
        }

        public async Task<Cliente?> GetOneAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Clientes
                .Include(c => c.Reservas)
                .FirstOrDefaultAsync(c => c.IdCliente == id);
        }

        public async Task<Cliente?> GetPorDocumentoAsync(string numeroDocumento)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.NumeroDocumento == numeroDocumento);
        }

        public async Task<Cliente> ModificarAsync(Cliente cliente)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == cliente.IdCliente);
            if (registro is null) return cliente;

            registro.Nombres = cliente.Nombres;
            registro.Apellidos = cliente.Apellidos;
            registro.TipoDocumento = cliente.TipoDocumento;
            registro.NumeroDocumento = cliente.NumeroDocumento;
            registro.Telefono = cliente.Telefono;
            registro.Email = cliente.Email;
            registro.Direccion = cliente.Direccion;
            registro.Activo = cliente.Activo;
            registro.MarcarModificacion();

            await context.SaveChangesAsync();
            return registro;
        }

        public async Task<IEnumerable<Cliente>> BuscarAsync(string criterio, bool incluirInactivos = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            criterio = criterio.Trim();
            var query = context.Clientes.AsNoTracking().Include(c => c.Reservas).AsQueryable();
            if (!incluirInactivos) query = query.Where(c => c.Activo);

            return await query
                .Where(c => c.Nombres.Contains(criterio)
                    || c.Apellidos.Contains(criterio)
                    || c.NumeroDocumento.Contains(criterio)
                    || (c.Telefono != null && c.Telefono.Contains(criterio))
                    || (c.Email != null && c.Email.Contains(criterio)))
                .OrderBy(c => c.Apellidos)
                .ThenBy(c => c.Nombres)
                .ToListAsync();
        }

        public async Task<bool> ActivarAsync(int id) => await CambiarActivoAsync(id, true);
        public async Task<bool> DesactivarAsync(int id) => await CambiarActivoAsync(id, false);

        private async Task<bool> CambiarActivoAsync(int id, bool activo)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var registro = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);
            if (registro is null) return false;
            registro.Activo = activo;
            registro.MarcarModificacion();
            await context.SaveChangesAsync();
            return true;
        }
    }
}
