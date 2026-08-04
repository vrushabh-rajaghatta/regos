using System.Text.Json.Serialization;
using RegOS.Api.Endpoints.Applications;
using RegOS.Api.Development;
using RegOS.Api.Endpoints.Authentication;
using RegOS.Api.Endpoints.Organization;
using RegOS.Api.Endpoints.OrganizationSites;
using RegOS.Api.Endpoints.Contacts;
using RegOS.Api.Endpoints.OrganizationDivisions;
using RegOS.Api.Endpoints.Platform;
using RegOS.Api.Endpoints.PlatformAdministration;
using RegOS.Api.Endpoints.MedicinalProducts;
using RegOS.Api.Endpoints.ProductDocuments;
using RegOS.Api.Endpoints.Products;
using RegOS.Api.Endpoints.ReferenceData;
using RegOS.Api.Endpoints.RegulatoryApplications;
using RegOS.Api.Endpoints.Submissions;
using RegOS.Api.Endpoints.ApplicationTypes;
using RegOS.Api.Endpoints.SubmissionTypes;
using RegOS.Api.Endpoints.SubmissionSubTypes;
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
using RegOS.ReferenceData.Infrastructure;
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
using RegOS.Interaction.Application;
using RegOS.Interaction.Infrastructure;
using RegOS.Registration.Application;
using RegOS.Registration.Infrastructure;
using RegOS.Study.Application;
using RegOS.Study.Infrastructure;
using RegOS.Api.Endpoints.Commitments;
using RegOS.Api.Endpoints.Inspections;
using RegOS.Api.Endpoints.Meetings;
using RegOS.Api.Endpoints.Correspondence;
using RegOS.Api.Endpoints.Registrations;
using RegOS.Api.Endpoints.Studies;
using RegOS.Api.Endpoints.Substances;
using RegOS.Api.Endpoints.Presentations;
using RegOS.Api.Endpoints.Components;
using RegOS.Api.Endpoints.GlobalLabels;
using RegOS.Labeling.Application;
using RegOS.Labeling.Infrastructure;

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
builder.Services.AddReferenceDataInfrastructure();

builder.Services.AddRegulatoryApplicationServices();
builder.Services.AddRegulatoryApplicationInfrastructure(builder.Configuration);

builder.Services.AddSubmissionApplication();
builder.Services.AddSubmissionInfrastructure();

builder.Services.AddProductDocumentApplication();
builder.Services.AddProductDocumentInfrastructure(builder.Configuration);

builder.Services.AddRegistrationApplication();
builder.Services.AddRegistrationInfrastructure();

builder.Services.AddInteractionApplication();
builder.Services.AddInteractionInfrastructure();

builder.Services.AddStudyApplication();
builder.Services.AddStudyInfrastructure();

builder.Services.AddLabelingApplication();
builder.Services.AddLabelingInfrastructure();

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

var referenceData = app.MapGroup("").WithTags("Reference Data");
referenceData.MapListCountries();
referenceData.MapListAuthorities();
referenceData.MapListApplicationTypes();
referenceData.MapListSubmissionTypes();
referenceData.MapListSubmissionSubTypes();
referenceData.MapListDocumentTypes();
referenceData.MapListRegulatoryTemplates();
referenceData.MapGetRegulatoryTemplate();
referenceData.MapListIdentifierSchemes();
referenceData.MapListContactRoles();
referenceData.MapListCorrespondenceTypes();
referenceData.MapListAuthorityDivisions();
// Its own list, not folded into the presentation vocabulary: a strength
// measures a quantity, and offering "vial" beside "mL" in one payload is the
// first step towards a formulation stating what the presentation already says.
referenceData.MapListMeasurementUnits();

var authentication = app.MapGroup("").WithTags("Authentication");
authentication.MapLogin();
authentication.MapRefreshSession();
authentication.MapLogout();
authentication.MapAcceptInvitation();
authentication.MapRequestPasswordReset();
authentication.MapCompletePasswordReset();
authentication.MapChangePassword();
authentication.MapSessions();
authentication.MapGetCurrentUser();

var organizations = app.MapGroup("").WithTags("Organizations");
organizations.MapActivateOrganization();
organizations.MapCreateOrganization();
organizations.MapDeactivateOrganization();
organizations.MapGetOrganization();
organizations.MapListOrganizations();
organizations.MapUpdateOrganization();
organizations.MapAddOrganizationIdentifier();
organizations.MapRemoveOrganizationIdentifier();

var organizationSites = app.MapGroup("").WithTags("Organization Sites");
organizationSites.MapCreateOrganizationSite();
organizationSites.MapGetOrganizationSite();
organizationSites.MapListOrganizationSites();
organizationSites.MapSiteDirectory();

var contacts = app.MapGroup("").WithTags("Contacts");
contacts.MapCreateContact();
contacts.MapGetContact();
contacts.MapListOrganizationContacts();
contacts.MapContactDirectory();

var organizationDivisions = app.MapGroup("").WithTags("Organization Divisions");
organizationDivisions.MapCreateOrganizationDivision();
organizationDivisions.MapListOrganizationDivisions();

var platformAdministration = app.MapGroup("").WithTags("Platform Administration");
platformAdministration.MapTenantAdministration();

var users = app.MapGroup("").WithTags("Users");
users.MapInviteUser();
users.MapResendInvitation();
users.MapListUsers();
users.MapGetUser();
users.MapUpdateUserProfile();
users.MapActivateUser();
users.MapDeactivateUser();

var products = app.MapGroup("").WithTags("Products");
products.MapRegisterProductEndpoint();
products.MapGetProductEndpoint();
products.MapUpdateProductEndpoint();
products.MapArchiveProductEndpoint();
products.MapListProductsEndpoint();

var medicinalProducts = app.MapGroup("").WithTags("Medicinal Products");
medicinalProducts.MapCreateMedicinalProduct();
medicinalProducts.MapListMedicinalProducts();
medicinalProducts.MapGetMedicinalProduct();
medicinalProducts.MapAddTradeName();
medicinalProducts.MapRemoveTradeName();
medicinalProducts.MapChangeMarketStatus();
medicinalProducts.MapActivateMedicinalProduct();
medicinalProducts.MapDeactivateMedicinalProduct();
medicinalProducts.MapRecordAtcCode();

var presentations = app.MapGroup("").WithTags("Presentations");
// Before the rest: /api/presentations/vocabulary must not be read as a
// /api/presentations/{id} that happens to be spelled "vocabulary".
presentations.MapGetPharmaceuticalVocabulary();
presentations.MapListPresentations();
presentations.MapAddPresentation();
presentations.MapRestatePresentation();
presentations.MapAddIngredient();
presentations.MapRestateIngredient();
presentations.MapRemoveIngredient();

var components = app.MapGroup("").WithTags("Components");
components.MapListComponents();
components.MapAddComponent();
components.MapRestateComponent();
components.MapMoveComponent();
components.MapRemoveComponent();

var globalLabels = app.MapGroup("").WithTags("Global Labels");
// Before the rest: /api/labels/vocabulary is its own route, and the label
// routes below are all /api/global-labels — no collision, but the vocabulary
// stays first for the same reason presentations' does.
globalLabels.MapGetLabelVocabulary();
globalLabels.MapListGlobalLabels();
globalLabels.MapCreateGlobalLabel();
globalLabels.MapListGlobalLabelVersions();
globalLabels.MapStartGlobalLabelDraft();
globalLabels.MapAttachGlobalLabelContent();
globalLabels.MapPublishGlobalLabelVersion();
globalLabels.MapDiscardGlobalLabelDraft();

var regulatoryApplications = app.MapGroup("").WithTags("Regulatory Applications");
regulatoryApplications.MapCreateRegulatoryApplication();
regulatoryApplications.MapRecordApplicationNumber();
regulatoryApplications.MapListRegulatoryApplications();
regulatoryApplications.MapGetApplication();
regulatoryApplications.MapGetApplicationContacts();

var submissions = app.MapGroup("").WithTags("Submissions");
submissions.MapCreateSubmission();
submissions.MapListSubmissions();
submissions.MapListContinuableSubmissions();
submissions.MapGetSubmission();
submissions.MapGenerateSequencePackage();
submissions.MapAttachProductDocument();
submissions.MapRemoveProductDocument();
submissions.MapPlaceSubmissionDocument();
submissions.MapChangeSubmissionFormat();
submissions.MapAssignSubmissionRole();
submissions.MapRemoveSubmissionRole();
submissions.MapListSubmissionRoles();
submissions.MapReportStudyOnPlacement();
submissions.MapListFileTags();
submissions.MapGetSubmissionContentPlan();
submissions.MapGetSubmissionChanges();
submissions.MapListSubmissionDocuments();
submissions.MapListAttachableProductDocuments();
submissions.MapValidateSubmission();
submissions.MapPublishSubmission();

var registrations = app.MapGroup("").WithTags("Registrations");
registrations.MapCreateRegistration();
registrations.MapRecordRegistrationApproval();
registrations.MapChangeRegistrationStatus();
registrations.MapGetRegistration();
registrations.MapListProductRegistrations();
registrations.MapListMarketRegistrations();
registrations.MapListRegistrationMarkets();
registrations.MapListExpiringRegistrations();

var correspondence = app.MapGroup("").WithTags("Correspondence");
correspondence.MapRecordCorrespondence();
correspondence.MapListCorrespondence();
correspondence.MapGetCorrespondence();
correspondence.MapAttachCorrespondenceContent();
correspondence.MapDownloadCorrespondenceContent();
correspondence.MapRemoveCorrespondenceContent();
correspondence.MapRaiseQuestion();
correspondence.MapRespondToQuestion();
correspondence.MapResolveQuestion();
correspondence.MapAssignQuestion();

var commitments = app.MapGroup("").WithTags("Commitments");
commitments.MapGiveCommitment();
commitments.MapListCommitments();
commitments.MapChangeCommitmentStatus();
commitments.MapListDueWork();

var meetings = app.MapGroup("").WithTags("Meetings");
meetings.MapBeginMeeting();
meetings.MapListMeetings();
meetings.MapChangeMeetingStatus();
meetings.MapRecordMeetingOutcome();

var inspections = app.MapGroup("").WithTags("Inspections");
inspections.MapBeginInspection();
inspections.MapListInspections();
inspections.MapChangeInspectionStatus();
inspections.MapRecordInspectionFindings();

var studies = app.MapGroup("").WithTags("Studies");
studies.MapRegisterClinicalStudy();
studies.MapRegisterNonClinicalStudy();
studies.MapListStudies();
studies.MapListStudyFilings();
studies.MapApplicationStudyCitations();

var substances = app.MapGroup("").WithTags("Substances");
// Before the list: /api/substances/vocabulary must not be read as a
// /api/substances/{id} that happens to be spelled "vocabulary".
substances.MapGetSubstanceVocabulary();
substances.MapListSubstances();
substances.MapCreateSubstance();
substances.MapListProductsContainingSubstance();

var productDocuments = app.MapGroup("").WithTags("Product Documents");
productDocuments.MapUploadProductDocument();
productDocuments.MapUploadDocumentVersion();
productDocuments.MapListProductDocuments();
productDocuments.MapGetProductDocument();
productDocuments.MapActivateProductDocument();
productDocuments.MapArchiveProductDocument();
productDocuments.MapGetProductDocumentUsage();

app.Run();
/// <summary>
/// Exposed so the integration tests can host this exact application through
/// <c>WebApplicationFactory</c>. Nothing else references it: the tests must
/// exercise the real pipeline — authentication handler, middleware order,
/// cookie writing — rather than a rebuilt approximation of it.
/// </summary>
public partial class Program;
