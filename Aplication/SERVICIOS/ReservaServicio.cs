using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;

namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public sealed class ReservaServicio : IReservaServicio
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ReservaServicio(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ResultadoOperacion<Reserva>> CrearReservaAsync(CrearReservaRequest request)
    {
        var validacion = ValidarRequest(request);
        if (!validacion.Exitoso)
        {
            return ResultadoOperacion<Reserva>.Error(validacion.Mensaje);
        }

        // EnableRetryOnFailure exige que toda transacción iniciada manualmente
        // se ejecute dentro de la estrategia de reintentos de EF Core.
        await using var strategyContext = await _contextFactory.CreateDbContextAsync();
        var executionStrategy = strategyContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            // Se crea un DbContext nuevo en cada intento para no reutilizar
            // entidades rastreadas si Azure SQL obliga a reintentar la operación.
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var noches = (request.FechaCheckOutProgramada.Date - request.FechaCheckInProgramada.Date).Days;
            var idsHabitaciones = request.HabitacionesSeleccionadas.Distinct().ToList();

            var habitaciones = await context.Habitaciones
                .Include(h => h.TipoHabitacion)
                .Where(h => idsHabitaciones.Contains(h.IdHabitacion))
                .ToListAsync();

            if (habitaciones.Count != idsHabitaciones.Count)
            {
                return ResultadoOperacion<Reserva>.Error("Una o más habitaciones seleccionadas no existen.");
            }

            var habitacionesNoDisponibles = habitaciones
                .Where(h => !h.Activo || h.EstadoHabitacion != EstadosHabitacion.Disponible)
                .Select(h => h.NumeroHabitacion)
                .ToList();

            if (habitacionesNoDisponibles.Count > 0)
            {
                return ResultadoOperacion<Reserva>.Error($"Las habitaciones {string.Join(", ", habitacionesNoDisponibles)} no están disponibles.");
            }

            var existeCruce = await context.DetallesReserva.AnyAsync(detalle =>
                idsHabitaciones.Contains(detalle.IdHabitacion)
                && detalle.Activo
                && detalle.Reserva.Activo
                && detalle.Reserva.EstadoReserva != EstadosReserva.Cancelada
                && request.FechaCheckInProgramada.Date < detalle.Reserva.FechaCheckOutProgramada.Date
                && request.FechaCheckOutProgramada.Date > detalle.Reserva.FechaCheckInProgramada.Date);

            if (existeCruce)
            {
                return ResultadoOperacion<Reserva>.Error("Existe cruce de fechas con una reserva activa para una de las habitaciones seleccionadas.");
            }

            var capacidadTotal = habitaciones.Sum(h => h.TipoHabitacion.CapacidadPersonas);
            if (request.CantidadHuespedes > capacidadTotal)
            {
                return ResultadoOperacion<Reserva>.Error($"La capacidad total seleccionada es {capacidadTotal}; no cubre {request.CantidadHuespedes} huéspedes.");
            }

            var cliente = await context.Clientes
                .FirstOrDefaultAsync(c => c.NumeroDocumento == request.Cliente.NumeroDocumento);

            if (cliente is null)
            {
                cliente = new Cliente
                {
                    Nombres = request.Cliente.Nombres,
                    Apellidos = request.Cliente.Apellidos,
                    TipoDocumento = request.Cliente.TipoDocumento,
                    NumeroDocumento = request.Cliente.NumeroDocumento,
                    Telefono = request.Cliente.Telefono,
                    Email = request.Cliente.Email,
                    Direccion = request.Cliente.Direccion,
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                };

                await context.Clientes.AddAsync(cliente);
                await context.SaveChangesAsync();
            }
            else
            {
                cliente.Nombres = request.Cliente.Nombres;
                cliente.Apellidos = request.Cliente.Apellidos;
                cliente.TipoDocumento = request.Cliente.TipoDocumento;
                cliente.Telefono = request.Cliente.Telefono;
                cliente.Email = request.Cliente.Email;
                cliente.Direccion = request.Cliente.Direccion;
                cliente.Activo = true;
                cliente.FechaModificacion = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            var reserva = new Reserva
            {
                FechaReserva = DateTime.UtcNow,
                FechaCheckInProgramada = request.FechaCheckInProgramada.Date,
                FechaCheckOutProgramada = request.FechaCheckOutProgramada.Date,
                CantidadHuespedes = request.CantidadHuespedes,
                EstadoReserva = EstadosReserva.Pendiente,
                Observaciones = request.Observaciones,
                IdCliente = cliente.IdCliente,
                TotalEstimado = habitaciones.Sum(h => h.TipoHabitacion.PrecioPorNoche * noches),
                Activo = true,
                FechaRegistro = DateTime.UtcNow
            };

            foreach (var habitacion in habitaciones)
            {
                reserva.DetallesReserva.Add(new DetalleReserva
                {
                    IdHabitacion = habitacion.IdHabitacion,
                    PrecioPorNoche = habitacion.TipoHabitacion.PrecioPorNoche,
                    CantidadNoches = noches,
                    SubTotal = habitacion.TipoHabitacion.PrecioPorNoche * noches,
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                });

                habitacion.EstadoHabitacion = EstadosHabitacion.Reservada;
                habitacion.FechaModificacion = DateTime.UtcNow;
            }

            await context.Reservas.AddAsync(reserva);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoOperacion<Reserva>.Ok(reserva, "Reserva registrada correctamente.");
        });
    }


    public async Task<ResultadoOperacion<Reserva>> ActualizarReservaAsync(ActualizarReservaRequest request)
    {
        var validacion = ValidarActualizacion(request);
        if (!validacion.Exitoso)
        {
            return ResultadoOperacion<Reserva>.Error(validacion.Mensaje);
        }

        await using var strategyContext = await _contextFactory.CreateDbContextAsync();
        var executionStrategy = strategyContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var reserva = await context.Reservas
                .Include(r => r.DetallesReserva)
                .FirstOrDefaultAsync(r => r.IdReserva == request.IdReserva && r.Activo);

            if (reserva is null)
            {
                return ResultadoOperacion<Reserva>.Error("La reserva no existe o está inactiva.");
            }

            if (reserva.EstadoReserva is not (EstadosReserva.Pendiente or EstadosReserva.Confirmada))
            {
                return ResultadoOperacion<Reserva>.Error("Solo se pueden editar reservas pendientes o confirmadas.");
            }

            var noches = (request.FechaCheckOutProgramada.Date - request.FechaCheckInProgramada.Date).Days;
            var idsHabitaciones = request.HabitacionesSeleccionadas.Distinct().ToList();

            var habitaciones = await context.Habitaciones
                .Include(h => h.TipoHabitacion)
                .Where(h => idsHabitaciones.Contains(h.IdHabitacion))
                .ToListAsync();

            if (habitaciones.Count != idsHabitaciones.Count)
            {
                return ResultadoOperacion<Reserva>.Error("Una o más habitaciones seleccionadas no existen.");
            }

            var idsActuales = reserva.DetallesReserva.Select(d => d.IdHabitacion).ToHashSet();
            var habitacionesNoDisponibles = habitaciones
                .Where(h => !h.Activo || (h.EstadoHabitacion != EstadosHabitacion.Disponible && !idsActuales.Contains(h.IdHabitacion)))
                .Select(h => h.NumeroHabitacion)
                .ToList();

            if (habitacionesNoDisponibles.Count > 0)
            {
                return ResultadoOperacion<Reserva>.Error($"Las habitaciones {string.Join(", ", habitacionesNoDisponibles)} no están disponibles.");
            }

            var existeCruce = await context.DetallesReserva.AnyAsync(detalle =>
                idsHabitaciones.Contains(detalle.IdHabitacion)
                && detalle.IdReserva != reserva.IdReserva
                && detalle.Activo
                && detalle.Reserva.Activo
                && detalle.Reserva.EstadoReserva != EstadosReserva.Cancelada
                && request.FechaCheckInProgramada.Date < detalle.Reserva.FechaCheckOutProgramada.Date
                && request.FechaCheckOutProgramada.Date > detalle.Reserva.FechaCheckInProgramada.Date);

            if (existeCruce)
            {
                return ResultadoOperacion<Reserva>.Error("Existe cruce de fechas con una reserva activa para una de las habitaciones seleccionadas.");
            }

            var capacidadTotal = habitaciones.Sum(h => h.TipoHabitacion.CapacidadPersonas);
            if (request.CantidadHuespedes > capacidadTotal)
            {
                return ResultadoOperacion<Reserva>.Error($"La capacidad total seleccionada es {capacidadTotal}; no cubre {request.CantidadHuespedes} huéspedes.");
            }

            var idsNuevos = idsHabitaciones.ToHashSet();
            var habitacionesLiberadas = await context.Habitaciones
                .Where(h => idsActuales.Contains(h.IdHabitacion) && !idsNuevos.Contains(h.IdHabitacion))
                .ToListAsync();

            foreach (var habitacion in habitacionesLiberadas)
            {
                habitacion.EstadoHabitacion = EstadosHabitacion.Disponible;
                habitacion.FechaModificacion = DateTime.UtcNow;
            }

            var detallesPorHabitacion = reserva.DetallesReserva.ToDictionary(d => d.IdHabitacion);
            var detallesEliminados = reserva.DetallesReserva
                .Where(d => !idsNuevos.Contains(d.IdHabitacion))
                .ToList();

            context.DetallesReserva.RemoveRange(detallesEliminados);

            reserva.FechaCheckInProgramada = request.FechaCheckInProgramada.Date;
            reserva.FechaCheckOutProgramada = request.FechaCheckOutProgramada.Date;
            reserva.CantidadHuespedes = request.CantidadHuespedes;
            reserva.Observaciones = request.Observaciones;
            reserva.TotalEstimado = habitaciones.Sum(h => h.TipoHabitacion.PrecioPorNoche * noches);
            reserva.FechaModificacion = DateTime.UtcNow;

            foreach (var habitacion in habitaciones)
            {
                if (detallesPorHabitacion.TryGetValue(habitacion.IdHabitacion, out var detalleExistente))
                {
                    detalleExistente.PrecioPorNoche = habitacion.TipoHabitacion.PrecioPorNoche;
                    detalleExistente.CantidadNoches = noches;
                    detalleExistente.SubTotal = habitacion.TipoHabitacion.PrecioPorNoche * noches;
                    detalleExistente.Activo = true;
                    detalleExistente.FechaModificacion = DateTime.UtcNow;
                }
                else
                {
                    reserva.DetallesReserva.Add(new DetalleReserva
                    {
                        IdHabitacion = habitacion.IdHabitacion,
                        PrecioPorNoche = habitacion.TipoHabitacion.PrecioPorNoche,
                        CantidadNoches = noches,
                        SubTotal = habitacion.TipoHabitacion.PrecioPorNoche * noches,
                        Activo = true,
                        FechaRegistro = DateTime.UtcNow
                    });
                }

                habitacion.EstadoHabitacion = EstadosHabitacion.Reservada;
                habitacion.FechaModificacion = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoOperacion<Reserva>.Ok(reserva, "Reserva actualizada correctamente.");
        });
    }

    public async Task<ResultadoOperacion> ConfirmarAsync(int idReserva)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var reserva = await context.Reservas.FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.Activo);
        if (reserva is null)
        {
            return ResultadoOperacion.Error("La reserva no existe o está inactiva.");
        }

        if (reserva.EstadoReserva != EstadosReserva.Pendiente)
        {
            return ResultadoOperacion.Error("Solo se pueden confirmar reservas pendientes.");
        }

        reserva.EstadoReserva = EstadosReserva.Confirmada;
        reserva.FechaModificacion = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Reserva confirmada correctamente.");
    }

    public async Task<ResultadoOperacion> RegistrarCheckInAsync(int idReserva, DateTime fechaCheckInReal)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var reserva = await context.Reservas
            .Include(r => r.DetallesReserva)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.Activo);

        if (reserva is null)
        {
            return ResultadoOperacion.Error("La reserva no existe o está inactiva.");
        }

        if (reserva.EstadoReserva is not (EstadosReserva.Pendiente or EstadosReserva.Confirmada))
        {
            return ResultadoOperacion.Error("Solo se puede registrar check-in en reservas pendientes o confirmadas.");
        }

        reserva.EstadoReserva = EstadosReserva.CheckIn;
        reserva.FechaCheckInReal = fechaCheckInReal;
        reserva.FechaModificacion = DateTime.UtcNow;

        var idsHabitaciones = reserva.DetallesReserva.Select(d => d.IdHabitacion).ToList();
        var habitaciones = await context.Habitaciones.Where(h => idsHabitaciones.Contains(h.IdHabitacion)).ToListAsync();
        foreach (var habitacion in habitaciones)
        {
            habitacion.EstadoHabitacion = EstadosHabitacion.Ocupada;
            habitacion.FechaModificacion = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Check-in registrado correctamente.");
    }

    public async Task<ResultadoOperacion> RegistrarCheckOutAsync(int idReserva, DateTime fechaCheckOutReal)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var reserva = await context.Reservas
            .Include(r => r.DetallesReserva)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.Activo);

        if (reserva is null)
        {
            return ResultadoOperacion.Error("La reserva no existe o está inactiva.");
        }

        if (reserva.EstadoReserva != EstadosReserva.CheckIn)
        {
            return ResultadoOperacion.Error("Solo se puede registrar check-out en reservas con check-in activo.");
        }

        reserva.EstadoReserva = EstadosReserva.Completada;
        reserva.FechaCheckOutReal = fechaCheckOutReal;
        reserva.FechaModificacion = DateTime.UtcNow;

        var idsHabitaciones = reserva.DetallesReserva.Select(d => d.IdHabitacion).ToList();
        var habitaciones = await context.Habitaciones.Where(h => idsHabitaciones.Contains(h.IdHabitacion)).ToListAsync();
        foreach (var habitacion in habitaciones)
        {
            habitacion.EstadoHabitacion = EstadosHabitacion.PendienteLimpieza;
            habitacion.FechaModificacion = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Check-out registrado. Las habitaciones quedan pendientes de limpieza.");
    }

    public async Task<ResultadoOperacion> CancelarAsync(int idReserva, string? motivoCancelacion)
    {
            await using var context = await _contextFactory.CreateDbContextAsync();
        var reserva = await context.Reservas
            .Include(r => r.DetallesReserva)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva && r.Activo);

        if (reserva is null)
        {
            return ResultadoOperacion.Error("La reserva no existe o está inactiva.");
        }

        if (reserva.EstadoReserva is EstadosReserva.Completada or EstadosReserva.Cancelada)
        {
            return ResultadoOperacion.Error("No se puede cancelar una reserva completada o ya cancelada.");
        }

        reserva.EstadoReserva = EstadosReserva.Cancelada;
        reserva.MotivoCancelacion = motivoCancelacion;
        reserva.FechaCancelacion = DateTime.UtcNow;
        reserva.FechaModificacion = DateTime.UtcNow;

        var idsHabitaciones = reserva.DetallesReserva.Select(d => d.IdHabitacion).ToList();
        var habitaciones = await context.Habitaciones.Where(h => idsHabitaciones.Contains(h.IdHabitacion)).ToListAsync();
        foreach (var habitacion in habitaciones.Where(h => h.EstadoHabitacion != EstadosHabitacion.Ocupada))
        {
            habitacion.EstadoHabitacion = EstadosHabitacion.Disponible;
            habitacion.FechaModificacion = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return ResultadoOperacion.Ok("Reserva cancelada correctamente.");
    }


    private static ResultadoOperacion ValidarActualizacion(ActualizarReservaRequest request)
    {
        if (request.IdReserva <= 0)
        {
            return ResultadoOperacion.Error("Debe seleccionar una reserva válida.");
        }

        if (request.FechaCheckOutProgramada.Date <= request.FechaCheckInProgramada.Date)
        {
            return ResultadoOperacion.Error("El check-out programado debe ser posterior al check-in programado.");
        }

        if (request.CantidadHuespedes <= 0)
        {
            return ResultadoOperacion.Error("La cantidad de huéspedes debe ser mayor que cero.");
        }

        if (request.HabitacionesSeleccionadas.Count == 0)
        {
            return ResultadoOperacion.Error("Debe seleccionar al menos una habitación.");
        }

        return ResultadoOperacion.Ok();
    }

    private static ResultadoOperacion ValidarRequest(CrearReservaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cliente.Nombres) || string.IsNullOrWhiteSpace(request.Cliente.Apellidos))
        {
            return ResultadoOperacion.Error("Debe registrar nombres y apellidos del cliente.");
        }

        if (string.IsNullOrWhiteSpace(request.Cliente.NumeroDocumento))
        {
            return ResultadoOperacion.Error("Debe registrar el documento del cliente.");
        }

        if (request.FechaCheckOutProgramada.Date <= request.FechaCheckInProgramada.Date)
        {
            return ResultadoOperacion.Error("El check-out programado debe ser posterior al check-in programado.");
        }

        if (request.HabitacionesSeleccionadas.Count == 0)
        {
            return ResultadoOperacion.Error("Debe seleccionar al menos una habitación.");
        }

        return ResultadoOperacion.Ok();
    }
}
