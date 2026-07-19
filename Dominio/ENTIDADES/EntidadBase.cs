using System.ComponentModel.DataAnnotations;
namespace GESTIONHOTELERA_V1_0_2.Data.ENTIDADES
{
    public abstract class EntidadBase
    {
        [Required]
        public bool Activo { get; set; } = true;

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? FechaModificacion { get; set; }
    }
}
