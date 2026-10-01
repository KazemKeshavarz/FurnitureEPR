using FurnitureEPR.Infrastructure.Identity;
using FurnitureEPR.Model.Categories;
using FurnitureEPR.Model.Components;
using FurnitureEPR.Model.Products;
using FurnitureEPR.Model.Customers;
using FurnitureEPR.Model.Orders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FurnitureEPR.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Component> Components => Set<Component>();
    public DbSet<ProductComponent> ProductComponents => Set<ProductComponent>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemComponent> OrderItemComponents => Set<OrderItemComponent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
