using Formify.Models;
using Microsoft.EntityFrameworkCore;

namespace Formify.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Авторизация
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        // Каталог
        public DbSet<Product> Products => Set<Product>();

        // Кастомные заявки
        public DbSet<CustomRequest> CustomRequests => Set<CustomRequest>();
        public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
        public DbSet<CustomRequestItem> CustomRequestItems => Set<CustomRequestItem>();
        public DbSet<CustomRequestImage> CustomRequestImages => Set<CustomRequestImage>();

        // Заказы (на будущее)
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<DeliveryMethod> DeliveryMethods => Set<DeliveryMethod>();

        // Корзина
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryPrice)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<CustomRequest>()
                .Property(r => r.FinalPrice)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<DeliveryMethod>()
                .Property(d => d.BasePrice)
                .HasColumnType("numeric(10,2)");
        }
    }
}
