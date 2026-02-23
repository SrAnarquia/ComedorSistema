using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ComedorSistema.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PedidoComidum> PedidoComida { get; set; }

    public virtual DbSet<Precio> Precios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoComidum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PedidoCo__3214EC075DB7ACC4");

            entity.ToTable("PedidoComida", "Pedidos");

            entity.Property(e => e.Cantidad).HasDefaultValue(1);
            entity.Property(e => e.Departamento)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FechaCompra).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdPersona).HasColumnName("Id_Persona");
            entity.Property(e => e.Nombre)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Precio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Precios__3214EC07EC8D5702");

            entity.ToTable("Precios", "Pedidos");

            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.Precio1)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Precio");
            entity.Property(e => e.PrecioAnterior).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
