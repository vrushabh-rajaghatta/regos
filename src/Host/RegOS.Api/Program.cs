using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RegOS.Api.Endpoints.Products;
using RegOS.Product.Application.DependencyInjection;
using RegOS.Product.Infrastructure.DependencyInjection;
using RegOS.Product.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

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
    app.UseCors(DevCorsPolicy);
}

app.MapRegisterProductEndpoint();
app.MapGetProductEndpoint();
app.MapListProductsEndpoint();

app.Run();