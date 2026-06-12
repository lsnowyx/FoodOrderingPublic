using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Request> Requests => Set<Request>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<MenuItemIngredient> MenuItemIngredients => Set<MenuItemIngredient>();
    public DbSet<MenuItemPicture> MenuItemPictures => Set<MenuItemPicture>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.ClientRequest)
                .WithOne(r => r.Client)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.WorkerRequest)
                .WithOne(r => r.Worker)
                .HasForeignKey(r => r.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Cart)
                .WithOne(c => c.Customer)
                .HasForeignKey<Cart>(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.AuditLogs)
                .WithOne(al => al.User)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Request>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Name)
                .IsRequired();

            entity.Property(r => r.Description)
                .IsRequired();
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name)
                .IsRequired();

            entity.HasMany(c => c.MenuItems)
                .WithOne(m => m.Category)
                .HasForeignKey(m => m.CategoryId);
        });

        builder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Name)
                .IsRequired();

            entity.Property(m => m.Price)
                .HasPrecision(18, 2);

            entity.HasMany(m => m.MenuItemIngredients)
                .WithOne(mi => mi.MenuItem)
                .HasForeignKey(mi => mi.MenuItemId);

            entity.HasMany(m => m.MenuItemPictures)
                .WithOne(p => p.MenuItem)
                .HasForeignKey(p => p.MenuItemId);
        });

        builder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.Name)
                .IsRequired();

            entity.HasMany(i => i.MenuItemIngredients)
                .WithOne(mi => mi.Ingredient)
                .HasForeignKey(mi => mi.IngredientId);
        });

        builder.Entity<MenuItemIngredient>(entity =>
        {
            entity.HasKey(mi => mi.Id);

            entity.HasIndex(mi => new { mi.MenuItemId, mi.IngredientId })
                .IsUnique();

            entity.Property(mi => mi.Quantity)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(mi => mi.MenuItem)
                .WithMany(m => m.MenuItemIngredients)
                .HasForeignKey(mi => mi.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(mi => mi.Ingredient)
                .WithMany(i => i.MenuItemIngredients)
                .HasForeignKey(mi => mi.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MenuItemPicture>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.ImageUrl)
                .IsRequired();
            entity.Property(p => p.ImagePublicId)
                .IsRequired();

            entity.HasOne(p => p.MenuItem)
                .WithMany(m => m.MenuItemPictures)
                .HasForeignKey(p => p.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.HasKey(ci => ci.Id);

            entity.HasOne(ci => ci.MenuItem)
                .WithMany()
                .HasForeignKey(ci => ci.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.HasOne(o => o.AssignedOrderManager)
                .WithMany()
                .HasForeignKey(o => o.AssignedOrderManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(oi => oi.Id);

            entity.HasOne(oi => oi.MenuItem)
                .WithMany()
                .HasForeignKey(oi => oi.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(al => al.Id);

            entity.Property(al => al.HttpMethod)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(al => al.Endpoint)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(al => al.RequestBody)
                .HasMaxLength(10000);

            entity.Property(al => al.ResponseBody)
                .HasMaxLength(10000);

            entity.Property(al => al.Timestamp)
                .IsRequired();

            entity.HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
