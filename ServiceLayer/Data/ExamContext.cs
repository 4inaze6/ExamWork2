using System;
using System.Collections.Generic;
using ServiceLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Data;

public partial class ExamContext : DbContext
{
    public ExamContext()
    {
    }

    public ExamContext(DbContextOptions<ExamContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExamManufacturer> ExamManufacturers { get; set; }

    public virtual DbSet<ExamOrder> ExamOrders { get; set; }

    public virtual DbSet<ExamOrderProduct> ExamOrderProducts { get; set; }

    public virtual DbSet<ExamPickupPoint> ExamPickupPoints { get; set; }

    public virtual DbSet<ExamProduct> ExamProducts { get; set; }

    public virtual DbSet<ExamRole> ExamRoles { get; set; }

    public virtual DbSet<ExamUser> ExamUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\MSSQLSERVER04;Database=Exam;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExamManufacturer>(entity =>
        {
            entity.HasKey(e => e.MunufacturerId);

            entity.ToTable("ExamManufacturer");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ExamOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__ExamOrde__C3905BAFC7CC9AED");

            entity.ToTable("ExamOrder");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.OrderDeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.OrderPickupPointNavigation).WithMany(p => p.ExamOrders)
                .HasForeignKey(d => d.OrderPickupPoint)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamOrder_ExamPickupPoint");

            entity.HasOne(d => d.User).WithMany(p => p.ExamOrders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ExamOrder_ExamUser");
        });

        modelBuilder.Entity<ExamOrderProduct>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductArticleNumber }).HasName("PK__ExamOrde__817A266255BBC081");

            entity.ToTable("ExamOrderProduct");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductArticleNumber).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithMany(p => p.ExamOrderProducts)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamOrder__Order__403A8C7D");

            entity.HasOne(d => d.ProductArticleNumberNavigation).WithMany(p => p.ExamOrderProducts)
                .HasForeignKey(d => d.ProductArticleNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExamOrder__Produ__412EB0B6");
        });

        modelBuilder.Entity<ExamPickupPoint>(entity =>
        {
            entity.HasKey(e => e.OrderPickupPoint);

            entity.ToTable("ExamPickupPoint");
        });

        modelBuilder.Entity<ExamProduct>(entity =>
        {
            entity.HasKey(e => e.ProductArticleNumber).HasName("PK__ExamProd__2EA7DCD5BF55BCD9");

            entity.ToTable("ExamProduct");

            entity.Property(e => e.ProductArticleNumber).HasMaxLength(100);
            entity.Property(e => e.ProductCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.ExamProducts)
                .HasForeignKey(d => d.ManufacturerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamProduct_ExamManufacturer");
        });

        modelBuilder.Entity<ExamRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ExamRole__8AFACE3AA2D40FB8");

            entity.ToTable("ExamRole");

            entity.Property(e => e.RoleId)
                .ValueGeneratedOnAdd()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(100);
        });

        modelBuilder.Entity<ExamUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__ExamUser__1788CCAC0829F7A9");

            entity.ToTable("ExamUser");

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.UserPatronymic).HasMaxLength(100);
            entity.Property(e => e.UserSurname).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.ExamUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamUser_ExamRole");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
