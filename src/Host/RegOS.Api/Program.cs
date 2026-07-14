using System.Text.Json.Serialization;
using RegOS.Api.Endpoints.Applications;
using RegOS.Api.Endpoints.MasterData;
using RegOS.Api.Endpoints.Organization;
using RegOS.Api.Endpoints.Products;
using RegOS.Api.Endpoints.RegulatoryApplications;
using RegOS.MasterData.Application;
using RegOS.MasterData.Infrastructure;
using RegOS.Organization.Application;
using RegOS.Organization.Infrastructure;
using RegOS.Persistence;
using RegOS.Persistence.Initialization;
using RegOS.Product.Application.DependencyInjection;
using RegOS.Product.Infrastructure.DependencyInjection;
using RegOS.RegulatoryApplication.Application;
using RegOS.RegulatoryApplication.Infrastructure;

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

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure();

builder.Services.AddMasterDataApplication();
builder.Services.AddMasterDataInfrastructure();

builder.Services.AddOrganizationApplication();
builder.Services.AddOrganizationInfrastructure();

builder.Services.AddRegulatoryApplicationServices();
builder.Services.AddRegulatoryApplicationInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializers = scope.ServiceProvider.GetServices<IDataInitializer>();

    foreach (var initializer in initializers)
    {
        await initializer.InitializeAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

app.MapListCountries();
app.MapListAuthorities();
app.MapListOrganizations();

app.MapRegisterProductEndpoint();
app.MapGetProductEndpoint();
app.MapListProductsEndpoint();

app.MapCreateRegulatoryApplication();
app.MapListRegulatoryApplications();
app.MapGetApplication();

app.Run();