using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    [Index(nameof(NumeroDocumento), IsUnique = true)]
    [Index(nameof(Email))]
    public class Cliente : EntidadBase
    {
        [Key]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los nombres no pueden tener más de 100 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos del cliente son obligatorios.")]
        [StringLength(100, ErrorMessage = "Los apellidos no pueden tener más de 100 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
        [StringLength(20, ErrorMessage = "El tipo de documento no puede tener más de 20 caracteres.")]
        public string TipoDocumento { get; set; } = "DNI";

        [Required(ErrorMessage = "El número de documento es obligatorio.")]
        [StringLength(30, ErrorMessage = "El número de documento no puede tener más de 30 caracteres.")]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede tener más de 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(150, ErrorMessage = "El correo no puede tener más de 150 caracteres.")]
        public string? Email { get; set; }

        [StringLength(250, ErrorMessage = "La dirección no puede tener más de 250 caracteres.")]
        public string? Direccion { get; set; }

        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
