using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GESTIONHOTELERA_V1_0_2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TipoHabitacion> TipoHabitaciones { get; set; } = null!;
        public DbSet<Habitacion> Habitaciones { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Reserva> Reservas { get; set; } = null!;
        public DbSet<DetalleReserva> DetallesReserva { get; set; } = null!;
        public DbSet<MetodoPago> MetodosPago { get; set; } = null!;
        public DbSet<Pago> Pagos { get; set; } = null!;
        public DbSet<Limpieza> Limpiezas { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoHabitacion>(entity =>
            {
                entity.Property(e => e.PrecioPorNoche).HasPrecision(18, 2);
                entity.HasMany(e => e.Habitaciones)
                    .WithOne(e => e.TipoHabitacion)
                    .HasForeignKey(e => e.IdTipoHabitacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Habitacion>(entity =>
            {
                entity.HasMany(e => e.DetallesReserva)
                    .WithOne(e => e.Habitacion)
                    .HasForeignKey(e => e.IdHabitacion)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Limpiezas)
                    .WithOne(e => e.Habitacion)
                    .HasForeignKey(e => e.IdHabitacion)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasMany(e => e.Reservas)
                    .WithOne(e => e.Cliente)
                    .HasForeignKey(e => e.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.Property(e => e.TotalEstimado).HasPrecision(18, 2);

                entity.HasMany(e => e.DetallesReserva)
                    .WithOne(e => e.Reserva)
                    .HasForeignKey(e => e.IdReserva)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Pagos)
                    .WithOne(e => e.Reserva)
                    .HasForeignKey(e => e.IdReserva)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DetalleReserva>(entity =>
            {
                entity.Property(e => e.PrecioPorNoche).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.Property(e => e.Monto).HasPrecision(18, 2);
            });

            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.HasMany(e => e.Pagos)
                    .WithOne(e => e.MetodoPago)
                    .HasForeignKey(e => e.IdMetodoPago)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { IdMetodoPago = 1, NombreMetodoPago = "Efectivo", DescripcionMetodoPago = "Pago registrado manualmente en efectivo", Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MetodoPago { IdMetodoPago = 2, NombreMetodoPago = "Tarjeta", DescripcionMetodoPago = "Pago registrado manualmente con tarjeta", Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MetodoPago { IdMetodoPago = 3, NombreMetodoPago = "Yape", DescripcionMetodoPago = "Pago registrado manualmente por Yape", Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MetodoPago { IdMetodoPago = 4, NombreMetodoPago = "Plin", DescripcionMetodoPago = "Pago registrado manualmente por Plin", Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new MetodoPago { IdMetodoPago = 5, NombreMetodoPago = "Transferencia", DescripcionMetodoPago = "Pago registrado manualmente por transferencia", Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            modelBuilder.Entity<TipoHabitacion>().HasData(
                new TipoHabitacion { IdTipoHabitacion = 1, NombreTipoHabitacion = "Simple", DescripcionTipoHabitacion = "Habitación para una persona", CapacidadPersonas = 1, PrecioPorNoche = 80m, Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TipoHabitacion { IdTipoHabitacion = 2, NombreTipoHabitacion = "Doble", DescripcionTipoHabitacion = "Habitación para dos personas", CapacidadPersonas = 2, PrecioPorNoche = 140m, Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new TipoHabitacion { IdTipoHabitacion = 3, NombreTipoHabitacion = "Suite", DescripcionTipoHabitacion = "Habitación premium con mayor comodidad", CapacidadPersonas = 4, PrecioPorNoche = 260m, Activo = true, FechaRegistro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }

    }
}
