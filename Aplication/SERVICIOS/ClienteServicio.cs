using GESTIONHOTELERA_V1_0_2.Data;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;


namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class ClienteServicio : IClienteServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IClienteRepositorio _clienteRepositorio;

    public ClienteServicio(IDbContextFactory<ApplicationDbContext> contextFactory, IClienteRepositorio clienteRepositorio)
    {
        _contextFactory = contextFactory;
        _clienteRepositorio = clienteRepositorio;
    }

    public async Task<IEnumerable<Cliente>> ObtenerTodosAsync(bool incluirInactivos = false)
    {
        return await _clienteRepositorio.GetAllAsync(incluirInactivos);
    }

    public async Task<IEnumerable<Cliente>> BuscarAsync(string criterio, bool incluirInactivos = false)
    {
        if (string.IsNullOrWhiteSpace(criterio))
        {
            return await ObtenerTodosAsync(incluirInactivos);
        }

        return await _clienteRepositorio.BuscarAsync(criterio, incluirInactivos);
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        return await _clienteRepositorio.GetOneAsync(id);
    }

    public async Task<Cliente?> ObtenerPorDocumentoAsync(string numeroDocumento)
    {
        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            return null;
        }

        return await _clienteRepositorio.GetPorDocumentoAsync(numeroDocumento.Trim());
    }

    public async Task<ResultadoOperacion<Cliente>> GuardarAsync(Cliente cliente)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        cliente.Nombres = cliente.Nombres.Trim();
        cliente.Apellidos = cliente.Apellidos.Trim();
        cliente.TipoDocumento = cliente.TipoDocumento.Trim().ToUpperInvariant();
        cliente.NumeroDocumento = cliente.NumeroDocumento.Trim();
        cliente.Telefono = string.IsNullOrWhiteSpace(cliente.Telefono) ? null : cliente.Telefono.Trim();
        cliente.Email = string.IsNullOrWhiteSpace(cliente.Email) ? null : cliente.Email.Trim().ToLowerInvariant();
        cliente.Direccion = string.IsNullOrWhiteSpace(cliente.Direccion) ? null : cliente.Direccion.Trim();

        if (string.IsNullOrWhiteSpace(cliente.Nombres))
        {
            return ResultadoOperacion<Cliente>.Error("Los nombres del cliente son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(cliente.Apellidos))
        {
            return ResultadoOperacion<Cliente>.Error("Los apellidos del cliente son obligatorios.");
        }

        if (string.IsNullOrWhiteSpace(cliente.TipoDocumento))
        {
            return ResultadoOperacion<Cliente>.Error("El tipo de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(cliente.NumeroDocumento))
        {
            return ResultadoOperacion<Cliente>.Error("El número de documento es obligatorio.");
        }

        var documentoDuplicado = await context.Clientes.AnyAsync(c =>
            c.IdCliente != cliente.IdCliente &&
            c.NumeroDocumento == cliente.NumeroDocumento);

        if (documentoDuplicado)
        {
            return ResultadoOperacion<Cliente>.Error("Ya existe un cliente con ese número de documento.");
        }

        if (!string.IsNullOrWhiteSpace(cliente.Email))
        {
            var emailDuplicado = await context.Clientes.AnyAsync(c =>
                c.IdCliente != cliente.IdCliente &&
                c.Email != null &&
                c.Email == cliente.Email);

            if (emailDuplicado)
            {
                return ResultadoOperacion<Cliente>.Error("Ya existe un cliente con ese correo electrónico.");
            }
        }

        if (cliente.IdCliente == 0)
        {
            cliente.Activo = true;
            cliente.FechaRegistro = DateTime.UtcNow;
            var creado = await _clienteRepositorio.AgregarAsync(cliente);
            return ResultadoOperacion<Cliente>.Ok(creado, "Cliente registrado correctamente.");
        }

        var actual = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == cliente.IdCliente);
        if (actual is null)
        {
            return ResultadoOperacion<Cliente>.Error("No se encontró el cliente a modificar.");
        }

        cliente.Activo = actual.Activo;
        var actualizado = await _clienteRepositorio.ModificarAsync(cliente);
        return ResultadoOperacion<Cliente>.Ok(actualizado, "Cliente actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> ActivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var cliente = await context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente is null)
        {
            return ResultadoOperacion.Error("No se encontró el cliente.");
        }

        if (cliente.Activo)
        {
            return ResultadoOperacion.Ok("El cliente ya estaba activo.");
        }

        await _clienteRepositorio.ActivarAsync(id);
        return ResultadoOperacion.Ok("Cliente activado correctamente.");
    }

    public async Task<ResultadoOperacion> DesactivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var cliente = await context.Clientes
            .Include(c => c.Reservas)
            .FirstOrDefaultAsync(c => c.IdCliente == id);

        if (cliente is null)
        {
            return ResultadoOperacion.Error("No se encontró el cliente.");
        }

        var tieneReservasActivas = cliente.Reservas.Any(r =>
            r.Activo &&
            r.EstadoReserva != EstadosReserva.Cancelada &&
            r.EstadoReserva != EstadosReserva.Completada);

        if (tieneReservasActivas)
        {
            return ResultadoOperacion.Error("No se puede desactivar el cliente porque tiene reservas activas o pendientes.");
        }

        await _clienteRepositorio.DesactivarAsync(id);
        return ResultadoOperacion.Ok("Cliente desactivado correctamente.");
    }
}
