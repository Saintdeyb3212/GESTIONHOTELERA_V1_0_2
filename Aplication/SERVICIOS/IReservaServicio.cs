using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;

using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS.MODELOS;
namespace GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

public interface IReservaServicio
{
    Task<ResultadoOperacion<Reserva>> CrearReservaAsync(CrearReservaRequest request);
    Task<ResultadoOperacion<Reserva>> ActualizarReservaAsync(ActualizarReservaRequest request);
    Task<ResultadoOperacion> ConfirmarAsync(int idReserva);
    Task<ResultadoOperacion> RegistrarCheckInAsync(int idReserva, DateTime fechaCheckInReal);
    Task<ResultadoOperacion> RegistrarCheckOutAsync(int idReserva, DateTime fechaCheckOutReal);
    Task<ResultadoOperacion> CancelarAsync(int idReserva, string? motivoCancelacion);
}
