using Microsoft.EntityFrameworkCore;
using RegOS.Api.Endpoints.Products;
using RegOS.Product.Application.DependencyInjection;
using RegOS.Product.Infrastructure.DependencyInjection;
using RegOS.Product.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("RegOS"));
});

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapRegisterProductEndpoint();
app.MapGetProductEndpoint();

app.Run();