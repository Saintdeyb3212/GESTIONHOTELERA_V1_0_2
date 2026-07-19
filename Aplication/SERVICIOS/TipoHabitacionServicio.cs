using GESTIONHOTELERA_V1_0_2.Data;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class TipoHabitacionServicio : ITipoHabitacionServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITipoHabitacionRepositorio _tipoHabitacionRepositorio;

    public TipoHabitacionServicio(IDbContextFactory<ApplicationDbContext> contextFactory, ITipoHabitacionRepositorio tipoHabitacionRepositorio)
    {
        _contextFactory = contextFactory;
        _tipoHabitacionRepositorio = tipoHabitacionRepositorio;
    }

    public async Task<IEnumerable<TipoHabitacion>> ObtenerTodosAsync(bool incluirInactivos = false)
    {
        return await _tipoHabitacionRepositorio.GetAllAsync(incluirInactivos);
    }

    public async Task<IEnumerable<TipoHabitacion>> BuscarAsync(string criterio, bool incluirInactivos = false)
    {
        if (string.IsNullOrWhiteSpace(criterio))
        {
            return await ObtenerTodosAsync(incluirInactivos);
        }

        return await _tipoHabitacionRepositorio.BuscarAsync(criterio, incluirInactivos);
    }

    public async Task<TipoHabitacion?> ObtenerPorIdAsync(int id)
    {
        return await _tipoHabitacionRepositorio.GetOneAsync(id);
    }

    public async Task<ResultadoOperacion<TipoHabitacion>> GuardarAsync(TipoHabitacion tipoHabitacion)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        tipoHabitacion.NombreTipoHabitacion = tipoHabitacion.NombreTipoHabitacion.Trim();
        tipoHabitacion.DescripcionTipoHabitacion = string.IsNullOrWhiteSpace(tipoHabitacion.DescripcionTipoHabitacion)
            ? null
            : tipoHabitacion.DescripcionTipoHabitacion.Trim();

        if (string.IsNullOrWhiteSpace(tipoHabitacion.NombreTipoHabitacion))
        {
            return ResultadoOperacion<TipoHabitacion>.Error("El nombre del tipo de habitación es obligatorio.");
        }

        if (tipoHabitacion.CapacidadPersonas <= 0)
        {
            return ResultadoOperacion<TipoHabitacion>.Error("La capacidad debe ser mayor que cero.");
        }

        if (tipoHabitacion.PrecioPorNoche <= 0)
        {
            return ResultadoOperacion<TipoHabitacion>.Error("El precio por noche debe ser mayor que cero.");
        }

        var nombreDuplicado = await context.TipoHabitaciones.AnyAsync(t =>
            t.IdTipoHabitacion != tipoHabitacion.IdTipoHabitacion &&
            t.NombreTipoHabitacion == tipoHabitacion.NombreTipoHabitacion);

        if (nombreDuplicado)
        {
            return ResultadoOperacion<TipoHabitacion>.Error("Ya existe un tipo de habitación con ese nombre.");
        }

        if (tipoHabitacion.IdTipoHabitacion == 0)
        {
            tipoHabitacion.Activo = true;
            tipoHabitacion.FechaRegistro = DateTime.UtcNow;
            var creado = await _tipoHabitacionRepositorio.AgregarAsync(tipoHabitacion);
            return ResultadoOperacion<TipoHabitacion>.Ok(creado, "Tipo de habitación registrado correctamente.");
        }

        var actualizado = await _tipoHabitacionRepositorio.ModificarAsync(tipoHabitacion);
        return ResultadoOperacion<TipoHabitacion>.Ok(actualizado, "Tipo de habitación actualizado correctamente.");
    }

    public async Task<ResultadoOperacion> ActivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var tipo = await context.TipoHabitaciones.FirstOrDefaultAsync(t => t.IdTipoHabitacion == id);
        if (tipo is null)
        {
            return ResultadoOperacion.Error("No se encontró el tipo de habitación.");
        }

        if (tipo.Activo)
        {
            return ResultadoOperacion.Ok("El tipo de habitación ya estaba activo.");
        }

        await _tipoHabitacionRepositorio.ActivarAsync(id);
        return ResultadoOperacion.Ok("Tipo de habitación activado correctamente.");
    }

    public async Task<ResultadoOperacion> DesactivarAsync(int id)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var tipo = await context.TipoHabitaciones
            .Include(t => t.Habitaciones)
            .FirstOrDefaultAsync(t => t.IdTipoHabitacion == id);

        if (tipo is null)
        {
            return ResultadoOperacion.Error("No se encontró el tipo de habitación.");
        }

        var habitacionesActivas = tipo.Habitaciones.Any(h => h.Activo);
        if (habitacionesActivas)
        {
            return ResultadoOperacion.Error("No se puede desactivar el tipo porque tiene habitaciones activas asociadas.");
        }

        await _tipoHabitacionRepositorio.DesactivarAsync(id);
        return ResultadoOperacion.Ok("Tipo de habitación desactivado correctamente.");
    }
}
