using FurnitureEPR.Application.Features.Categories;
using FurnitureEPR.Application.Features.Components;
using FurnitureEPR.Application.Features.Customers;
using FurnitureEPR.Application.Features.Orders;
using FurnitureEPR.Application.Features.Products;
using FurnitureEPR.Infrastructure.Persistence;
using FurnitureEPR.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FurnitureEPR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<IComponentRepository, ComponentRepository>();
        services.AddScoped<IComponentReadRepository, ComponentReadRepository>();
        services.AddScoped<IProductComponentRepository, ProductComponentRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderReadRepository, OrderReadRepository>();
        services.AddScoped<IOrderFinalizationRepository, OrderFinalizationRepository>();
        services.AddScoped<IOrderDraftRepository, OrderDraftRepository>();
        services.AddScoped<IOrderWorkflowRuntimeRepository, OrderWorkflowRuntimeRepository>();

        return services;
    }
}
