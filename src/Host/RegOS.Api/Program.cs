using System.Text.Json.Serialization;
using RegOS.Api.Endpoints.Applications;
using RegOS.Api.Development;
using RegOS.Api.Endpoints.Authentication;
using RegOS.Api.Endpoints.Organization;
using RegOS.Api.Endpoints.Platform;
using RegOS.Api.Endpoints.PlatformAdministration;
using RegOS.Api.Endpoints.ProductDocuments;
using RegOS.Api.Endpoints.Products;
using RegOS.Api.Endpoints.ReferenceData;
using RegOS.Api.Endpoints.RegulatoryApplications;
using RegOS.Api.Endpoints.Submissions;
using RegOS.Api.Endpoints.SubmissionTypes;
using RegOS.Organization.Application;
using RegOS.Organization.Infrastructure;
using RegOS.Platform.Application;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Infrastructure;
using RegOS.Api.Authentication;
using RegOS.Api.Middleware;
using RegOS.Api.Tenancy;
using RegOS.SharedKernel.Abstractions;
using RegOS.ReferenceData.Application;
using RegOS.Persistence;
using RegOS.Persistence.Initialization;
using RegOS.Product.Application.DependencyInjection;
using RegOS.Product.Infrastructure.DependencyInjection;
using RegOS.RegulatoryApplication.Application;
using RegOS.RegulatoryApplication.Infrastructure;
using RegOS.Submission.Application;
using RegOS.Submission.Infrastructure;
using RegOS.ProductDocument.Application;
using RegOS.ProductDocument.Infrastructure;

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
            .AllowAnyMethod()
            // Required for the session cookies to travel at all: the browser
            // withholds them from cross-origin requests otherwise. Only legal
            // beside a specific origin, never AllowAnyOrigin.
            .AllowCredentials();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// Tenant context is scoped: one per request, resolved from the authenticated
// caller. Registered before the modules so every handler can depend on it.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, ClaimsTenantContext>();

builder.Services.AddRegOSAuthentication();

builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure();

builder.Services.AddOrganizationApplication();
builder.Services.AddOrganizationInfrastructure();

builder.Services.AddPlatformApplication();
builder.Services.AddPlatformInfrastructure(builder.Configuration);

// Development only, and guarded here rather than inside the notifier: it writes
// a live acceptance token to the log, and that guarantee should be readable
// where it is wired up. Registered after the default so it replaces it.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<
        IInvitationNotifier, DevelopmentInvitationNotifier>();

    builder.Services.AddScoped<
        IPasswordResetNotifier, DevelopmentPasswordResetNotifier>();
}

builder.Services.AddReferenceDataApplication();

builder.Services.AddRegulatoryApplicationServices();
builder.Services.AddRegulatoryApplicationInfrastructure(builder.Configuration);

builder.Services.AddSubmissionApplication();
builder.Services.AddSubmissionInfrastructure();

builder.Services.AddProductDocumentApplication();
builder.Services.AddProductDocumentInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializers = scope.ServiceProvider.GetServices<IDataInitializer>();

    foreach (var initializer in initializers)
    {
        await initializer.InitializeAsync();
    }

    // Development only, and deliberately guarded here rather than inside the
    // seeders: accounts with known passwords must never be created anywhere
    // else, and that guarantee should be readable at the call site.
    if (app.Environment.IsDevelopment())
    {
        await DevelopmentCredentialSeeder.SeedAsync(scope.ServiceProvider);
        await PlatformAdministratorSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(DevCorsPolicy);
}

// After CORS so a rejected preflight never reaches the token handler, and
// before the endpoints so RequireAuthorization has an identity to inspect.
app.UseAuthentication();
app.UseAuthorization();

app.MapListCountries();
app.MapListAuthorities();
app.MapLogin();
app.MapRefreshSession();
app.MapLogout();
app.MapAcceptInvitation();
app.MapRequestPasswordReset();
app.MapCompletePasswordReset();
app.MapChangePassword();
app.MapSessions();
app.MapGetCurrentUser();

app.MapActivateOrganization();
app.MapCreateOrganization();
app.MapDeactivateOrganization();
app.MapGetOrganization();
app.MapListOrganizations();
app.MapUpdateOrganization();

app.MapTenantAdministration();

app.MapInviteUser();
app.MapResendInvitation();
app.MapListUsers();
app.MapGetUser();
app.MapUpdateUserProfile();
app.MapActivateUser();
app.MapDeactivateUser();
app.MapListSubmissionTypes();
app.MapListDocumentTypes();

app.MapRegisterProductEndpoint();
app.MapGetProductEndpoint();
app.MapUpdateProductEndpoint();
app.MapArchiveProductEndpoint();
app.MapListProductsEndpoint();

app.MapCreateRegulatoryApplication();
app.MapListRegulatoryApplications();
app.MapGetApplication();

app.MapCreateSubmission();
app.MapListSubmissions();
app.MapGetSubmission();
app.MapAttachProductDocument();
app.MapRemoveProductDocument();
app.MapListSubmissionDocuments();
app.MapListAttachableProductDocuments();
app.MapValidateSubmission();
app.MapPublishSubmission();
app.MapGetSubmissionSnapshot();

app.MapUploadProductDocument();
app.MapListProductDocuments();
app.MapGetProductDocument();
app.MapActivateProductDocument();
app.MapArchiveProductDocument();
app.MapGetProductDocumentUsage();

app.Run();
/// <summary>
/// Exposed so the integration tests can host this exact application through
/// <c>WebApplicationFactory</c>. Nothing else references it: the tests must
/// exercise the real pipeline — authentication handler, middleware order,
/// cookie writing — rather than a rebuilt approximation of it.
/// </summary>
public partial class Program;
