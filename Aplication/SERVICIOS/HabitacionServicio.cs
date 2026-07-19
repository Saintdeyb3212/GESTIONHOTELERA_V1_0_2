using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
using GESTIONHOTELERA_V1_0_2.Data;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class HabitacionServicio : IHabitacionServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IHabitacionRepositorio _habitacionRepositorio;

    public HabitacionServicio(IDbContextFactory<ApplicationDbContext> contextFactory, IHabitacionRepositorio habitacionRepositorio)
    {
        _contextFactory = contextFactory;
        _habitacionRepositorio = habitacionRepositorio;
    }

    public async Task<IEnumerable<Habitacion>> ObtenerTodasAsync(bool incluirInactivos = false)
    {
        return await _habitacionRepositorio.GetAllAsync(incluirInactivos);
    }

    public async Task<IEnumerable<Habitacion>> BuscarAsync(string criterio, bool incluirInactivos = false)
    {
        if (string.IsNullOrWhiteSpace(criterio))
        {
            return await ObtenerTodasAsync(incluirInactivos);
        }

        return await _habitacionRepositorio.BuscarAsync(criterio, incluirInactivos);
    }

    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await _habitacionRepositorio.GetOneAsync(id);
    }

    public async Task<ResultadoOperacion<Habitacion>> GuardarAsync(Habitacion habitacion)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        habitacion.NumeroHabitacion = habitacion.NumeroHabitacion.Trim();
        habitacion.Piso = string.IsNullOrWhiteSpace(habitacion.Piso) ? null : habitacion.Piso.Trim();
        habitacion.DescripcionHabitacion = string.IsNullOrWhiteSpace(habitacion.DescripcionHabitacion)
            ? null
            : habitacion.DescripcionHabitacion.Trim();

        if (string.IsNullOrWhiteSpace(habitacion.NumeroHabitacion))
        {
            return ResultadoOperacion<Habitacion>.Error("El número de habitación es obligatorio.");
        }

        var tipoExiste = await context.TipoHabitaciones.AnyAsync(t => t.IdTipoHabitacion == habitacion.IdTipoHabitacion && t.Activo);
        if (!tipoExiste)
        {
            return ResultadoOperacion<Habitacion>.Error("Debes seleccionar un tipo de habitación activo.");
        }

        var numeroDuplicado = await context.Habitaciones.AnyAsync(h =>
            h.IdHabitacion != habitacion.IdHabitacion &&
            h.NumeroHabitacion == habitacion.NumeroHabitacion);

        if (numeroDuplicado)
        {
            return ResultadoOperacion<Habitacion>.Error("Ya existe una habitación con ese número.");
        }

        if (habitacion.IdHabitacion == 0)
        {
            habitacion.Activo = true;
            habitacion.EstadoHabitacion = EstadosHabitacion.Disponible;
            habitacion.FechaRegistro = DateTime.UtcNow;
            var creada = await _habitacionRepositorio.AgregarAsync(habitacion);
            return ResultadoOperacion<Habitacion>.Ok(creada, "Habitación registrada correctamente.");
        }

        var actual = await context.Habitaciones.AsNoTracking().FirstOrDefaultAsync(h => h.IdHabitacion == habitacion.IdHabitacion);
        if (actual is null)
        {
            return ResultadoOperacion<Habitacion>.Error("No se encontró la habitación a modificar.");
        }

        var estadosManuales = new[]
        {
            EstadosHabitacion.Disponible,
            EstadosHabitacion.Mantenimiento,
            EstadosHabitacion.PendienteLimpieza,
            EstadosHabitacion.EnLimpieza
        };

        if (!actual.Activo)
        {
            habitacion.EstadoHabitacion = EstadosHabitacion.Inactiva;
        }
        else if (!estadosManuales.Contains(actual.EstadoHabitacion))
        {
            habitacion.EstadoHabitacion = actual.EstadoHabitacion;
        }
        else if (!estadosManuales.Contains(habitacion.EstadoHabitacion))
        {
            habitacion.EstadoHabitacion = actual.EstadoHabitacion;
        }

        habitacion.Activo = actual.Activo;
        var actualizada = await _habitacionRepositorio.ModificarAsync(habitacion);
        return ResultadoOperacion<Habitacion>.Ok(actualizada, "Habitación actualizada correctamente.");
    }

    public async Task<ResultadoOperacion> ActivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (habitacion is null)
        {
            return ResultadoOperacion.Error("No se encontró la habitación.");
        }

        if (habitacion.Activo)
        {
            return ResultadoOperacion.Ok("La habitación ya estaba activa.");
        }

        var tipoActivo = await context.TipoHabitaciones.AnyAsync(t => t.IdTipoHabitacion == habitacion.IdTipoHabitacion && t.Activo);
        if (!tipoActivo)
        {
            return ResultadoOperacion.Error("No se puede activar porque su tipo de habitación está inactivo.");
        }

        await _habitacionRepositorio.ActivarAsync(id);
        return ResultadoOperacion.Ok("Habitación activada correctamente.");
    }

    public async Task<ResultadoOperacion> DesactivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var habitacion = await context.Habitaciones
            .Include(h => h.DetallesReserva)
                .ThenInclude(d => d.Reserva)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);

        if (habitacion is null)
        {
            return ResultadoOperacion.Error("No se encontró la habitación.");
        }

        var tieneReservaActiva = habitacion.DetallesReserva.Any(d =>
            d.Activo &&
            d.Reserva.Activo &&
            d.Reserva.EstadoReserva != EstadosReserva.Cancelada &&
            d.Reserva.EstadoReserva != EstadosReserva.Completada);

        if (tieneReservaActiva)
        {
            return ResultadoOperacion.Error("No se puede desactivar la habitación porque tiene reservas activas o pendientes.");
        }

        await _habitacionRepositorio.DesactivarAsync(id);
        return ResultadoOperacion.Ok("Habitación desactivada correctamente.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(int id, string estadoHabitacion)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var estadosPermitidos = new[]
        {
            EstadosHabitacion.Disponible,
            EstadosHabitacion.Mantenimiento,
            EstadosHabitacion.PendienteLimpieza,
            EstadosHabitacion.EnLimpieza
        };

        if (!estadosPermitidos.Contains(estadoHabitacion))
        {
            return ResultadoOperacion.Error("El estado seleccionado no puede asignarse manualmente desde este módulo.");
        }

        var habitacion = await context.Habitaciones.FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (habitacion is null)
        {
            return ResultadoOperacion.Error("No se encontró la habitación.");
        }

        if (!habitacion.Activo)
        {
            return ResultadoOperacion.Error("No se puede cambiar el estado de una habitación inactiva.");
        }

        await _habitacionRepositorio.CambiarEstadoAsync(id, estadoHabitacion);
        return ResultadoOperacion.Ok("Estado de habitación actualizado correctamente.");
    }
}
