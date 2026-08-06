import { RegulatoryLayout } from "@/features/regulatory/layout/RegulatoryLayout";
import { createBrowserRouter } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AppLayout } from "@/shared/layout/AppLayout";
import { ProductListPage } from "@/features/regulatory/products/pages/ProductListPage";
import { ProductWorkspaceLayout } from "@/features/regulatory/products/layout/ProductWorkspaceLayout";
import { ProductOverviewPage } from "@/features/regulatory/products/pages/ProductOverviewPage";
import { RegulatoryApplicationListPage } from "@/features/regulatory/applications/pages/RegulatoryApplicationListPage";
import { ApplicationWorkspaceLayout } from "@/features/regulatory/applications/layout/ApplicationWorkspaceLayout";
import { ApplicationOverviewPage } from "@/features/regulatory/applications/pages/ApplicationOverviewPage";
import { ApplicationSubmissionsPage } from "@/features/regulatory/applications/pages/ApplicationSubmissionsPage";
import { ApplicationDocumentsPage } from "@/features/regulatory/applications/pages/ApplicationDocumentsPage";
import { ApplicationPublishingPage } from "@/features/regulatory/applications/pages/ApplicationPublishingPage";
import { ApplicationHistoryPage } from "@/features/regulatory/applications/pages/ApplicationHistoryPage";
import { ApplicationStudiesPage } from "@/features/regulatory/applications/pages/ApplicationStudiesPage";
import { SubmissionWorkspaceLayout } from "@/features/regulatory/submissions/layout/SubmissionWorkspaceLayout";
import { SubmissionOverviewPage } from "@/features/regulatory/submissions/pages/SubmissionOverviewPage";
import { SubmissionDocumentsPage } from "@/features/regulatory/submissions/pages/SubmissionDocumentsPage";
import { SubmissionContentPlanPage } from "@/features/regulatory/submissions/pages/SubmissionContentPlanPage";
import { SubmissionValidationPage } from "@/features/regulatory/submissions/pages/SubmissionValidationPage";
import { SubmissionPublishingPage } from "@/features/regulatory/submissions/pages/SubmissionPublishingPage";
import { SubmissionChangesPage } from "@/features/regulatory/submissions/pages/SubmissionChangesPage";
import { SubmissionHistoryPage } from "@/features/regulatory/submissions/pages/SubmissionHistoryPage";
import { SubmissionPeoplePage } from "@/features/regulatory/submissions/pages/SubmissionPeoplePage";
import { ProductDocumentsListPage } from "@/features/regulatory/documents/pages/ProductDocumentsListPage";
import { DocumentWorkspaceLayout } from "@/features/regulatory/documents/layout/DocumentWorkspaceLayout";
import { DocumentOverviewPage } from "@/features/regulatory/documents/pages/DocumentOverviewPage";
import { DocumentVersionsPage } from "@/features/regulatory/documents/pages/DocumentVersionsPage";
import { DocumentUsagePage } from "@/features/regulatory/documents/pages/DocumentUsagePage";
import { DocumentHistoryPage } from "@/features/regulatory/documents/pages/DocumentHistoryPage";
import { DocumentAiInsightsPage } from "@/features/regulatory/documents/pages/DocumentAiInsightsPage";
import { PlatformLayout } from "@/features/platform/layout/PlatformLayout";
import { OrganizationWorkspaceLayout } from "@/features/regulatory/organizations/layout/OrganizationWorkspaceLayout";
import { OrganizationOverviewPage } from "@/features/regulatory/organizations/pages/OrganizationOverviewPage";
import { OrganizationDivisionsPage } from "@/features/regulatory/organizations/pages/OrganizationDivisionsPage";
import { OrganizationSitesPage } from "@/features/regulatory/organizations/pages/OrganizationSitesPage";
import { OrganizationContactsPage } from "@/features/regulatory/organizations/pages/OrganizationContactsPage";
import { DueWorkPage } from "@/features/regulatory/dueWork/pages/DueWorkPage";
import { MeetingsPage } from "@/features/regulatory/meetings/pages/MeetingsPage";
import { InspectionsPage } from "@/features/regulatory/inspections/pages/InspectionsPage";
import { StudiesPage } from "@/features/regulatory/studies/pages/StudiesPage";
import { SubstancesPage } from "@/features/regulatory/substances/pages/SubstancesPage";
import { CorrespondencePage } from "@/features/regulatory/correspondence/pages/CorrespondencePage";
import { CorrespondenceDetailPage } from "@/features/regulatory/correspondence/pages/CorrespondenceDetailPage";
import { OrganizationsPage } from "@/features/regulatory/organizations/pages/OrganizationsPage";
import { SiteDirectoryPage } from "@/features/regulatory/organizations/pages/SiteDirectoryPage";
import { ContactDirectoryPage } from "@/features/regulatory/organizations/pages/ContactDirectoryPage";
import { TenantsPage } from "@/features/platform/tenants/pages/TenantsPage";
import { PlatformIndexRedirect } from "@/features/platform/layout/PlatformIndexRedirect";
import { UsersPage } from "@/features/platform/users/pages/UsersPage";
import { UserDetailsPage } from "@/features/platform/users/pages/UserDetailsPage";
import { Navigate } from "react-router-dom";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { AcceptInvitationPage } from "@/features/auth/pages/AcceptInvitationPage";
import { ForgotPasswordPage } from "@/features/auth/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "@/features/auth/pages/ResetPasswordPage";
import { SettingsLayout } from "@/features/settings/layout/SettingsLayout";
import { SecurityPage } from "@/features/settings/pages/SecurityPage";
import { SessionsPage } from "@/features/settings/pages/SessionsPage";
import { RequireAuth } from "@/features/auth/components/RequireAuth";
import { TemplateListPage } from "@/features/regulatory/templates/pages/TemplateListPage";
import { TemplateDetailPage } from "@/features/regulatory/templates/pages/TemplateDetailPage";
import { PlaybookListPage } from "@/features/regulatory/playbooks/pages/PlaybookListPage";
import { PlaybookDetailPage } from "@/features/regulatory/playbooks/pages/PlaybookDetailPage";
import { ProductLabelsPage } from "@/features/regulatory/labels/pages/ProductLabelsPage";
import { ProductRegistrationsPage } from "@/features/regulatory/registrations/pages/ProductRegistrationsPage";
import { MedicinalProductPage } from "@/features/regulatory/medicinalProducts/pages/MedicinalProductPage";
import { RegistrationMarketsPage } from "@/features/regulatory/registrations/pages/RegistrationMarketsPage";
import { MarketRegistrationsPage } from "@/features/regulatory/registrations/pages/MarketRegistrationsPage";
import { RegistrationDetailPage } from "@/features/regulatory/registrations/pages/RegistrationDetailPage";

export const router = createBrowserRouter([
  {
    // Outside the shell: there is no navigation to show someone who has not
    // signed in, and the header links to pages they cannot load.
    path: "/login",
    element: <LoginPage />,
  },
  {
    // Outside RequireAuth: whoever follows this link has no session, and
    // obtaining the ability to have one is the point.
    path: "/accept-invitation",
    element: <AcceptInvitationPage />,
  },
  {
    // Also outside RequireAuth, and for a stronger reason: someone who has
    // forgotten their password cannot sign in to ask for a new one.
    path: "/forgot-password",
    element: <ForgotPasswordPage />,
  },
  {
    path: "/reset-password",
    element: <ResetPasswordPage />,
  },
  {
    path: "/",
    element: <RequireAuth />,
    children: [
      {
        element: <AppLayout />,
        children: [
          {
            index: true,
            element: <HomePage />,
          },
          {
            path: "settings",
            element: <SettingsLayout />,
            children: [
              {
                index: true,
                element: <Navigate to="security" replace />,
              },
              {
                path: "security",
                element: <SecurityPage />,
              },
              {
                path: "sessions",
                element: <SessionsPage />,
              },
            ],
          },
          {
            path: "platform",
            element: <PlatformLayout />,
            children: [
              {
                index: true,
                element: <PlatformIndexRedirect />,
              },
              {
                path: "tenants",
                element: <TenantsPage />,
              },
              {
                path: "users",
                element: <UsersPage />,
              },
              {
                path: "users/:userId",
                element: <UserDetailsPage />,
              },
            ],
          },
          {
            path: "regulatory",
            element: <RegulatoryLayout />,
            children: [
              {
                path: "playbooks",
                children: [
                  {
                    index: true,
                    element: <PlaybookListPage />,
                  },
                  {
                    path: ":playbookId",
                    element: <PlaybookDetailPage />,
                  },
                ],
              },
              {
                path: "templates",
                children: [
                  {
                    index: true,
                    element: <TemplateListPage />,
                  },
                  {
                    path: ":templateId",
                    element: <TemplateDetailPage />,
                  },
                ],
              },
              {
                path: "products",
                children: [
                  {
                    index: true,
                    element: <ProductListPage />,
                  },
                  // Product Workspace — portfolio-level view.
                  {
                    path: ":globalProductId",
                    element: <ProductWorkspaceLayout />,
                    children: [
                      {
                        index: true,
                        element: <ProductOverviewPage />,
                      },
                      {
                        path: "applications",
                        children: [
                          {
                            index: true,
                            element: <RegulatoryApplicationListPage />,
                          },
                        ],
                      },
                      {
                        path: "documents",
                        element: <ProductDocumentsListPage />,
                      },
                      {
                        path: "registrations",
                        element: <ProductRegistrationsPage />,
                      },
                      // The labels held above any market (EPIC-018 S001). A
                      // sibling of registrations rather than a child: what the
                      // company says and what a regulator has authorised are
                      // different facts on different clocks (ADR-059).
                      {
                        path: "labels",
                        element: <ProductLabelsPage />,
                      },
                      // One market, as its own working surface (EPIC-017 S004).
                      // Nested inside the product workspace rather than beside
                      // it: a market is always read in the context of the
                      // product it localises, and the sidebar should stay.
                      {
                        path: "markets/:medicinalProductId",
                        element: <MedicinalProductPage />,
                      },
                    ],
                  },
                  // Application Workspace — execution-level view. Nested under the
                  // product URL, but a full-screen sibling so its sidebar replaces
                  // the product sidebar (rather than rendering two sidebars).
                  {
                    path: ":globalProductId/applications/:applicationId",
                    element: <ApplicationWorkspaceLayout />,
                    children: [
                      {
                        index: true,
                        element: <ApplicationOverviewPage />,
                      },
                      {
                        path: "submissions",
                        element: <ApplicationSubmissionsPage />,
                      },
                      {
                        path: "documents",
                        element: <ApplicationDocumentsPage />,
                      },
                      {
                        path: "publishing",
                        element: <ApplicationPublishingPage />,
                      },
                      // "Which studies support this filing?" — a claim the
                      // application makes, so it lives in the application's
                      // workspace rather than on the study (ADR-056).
                      {
                        path: "studies",
                        element: <ApplicationStudiesPage />,
                      },
                      {
                        path: "history",
                        element: <ApplicationHistoryPage />,
                      },
                    ],
                  },
                  // Submission Workspace — deepest execution context. Also a
                  // full-screen sibling so its sidebar replaces the application
                  // sidebar. The URL preserves the full business hierarchy:
                  // product -> application -> submission.
                  {
                    path: ":globalProductId/applications/:applicationId/submissions/:submissionId",
                    element: <SubmissionWorkspaceLayout />,
                    children: [
                      {
                        index: true,
                        element: <SubmissionOverviewPage />,
                      },
                      {
                        path: "documents",
                        element: <SubmissionDocumentsPage />,
                      },
                      {
                        path: "content-plan",
                        element: <SubmissionContentPlanPage />,
                      },
                      {
                        path: "validation",
                        element: <SubmissionValidationPage />,
                      },
                      {
                        path: "publishing",
                        element: <SubmissionPublishingPage />,
                      },
                      {
                        path: "people",
                        element: <SubmissionPeoplePage />,
                      },
                      {
                        path: "changes",
                        element: <SubmissionChangesPage />,
                      },
                      {
                        path: "history",
                        element: <SubmissionHistoryPage />,
                      },
                    ],
                  },
                  // Product Document Workspace — full-screen sibling under the
                  // product URL. Explicit sub-routes; index redirects to overview.
                  {
                    path: ":globalProductId/documents/:documentId",
                    element: <DocumentWorkspaceLayout />,
                    children: [
                      {
                        index: true,
                        element: <Navigate to="overview" replace />,
                      },
                      {
                        path: "overview",
                        element: <DocumentOverviewPage />,
                      },
                      {
                        path: "versions",
                        element: <DocumentVersionsPage />,
                      },
                      {
                        path: "usage",
                        element: <DocumentUsagePage />,
                      },
                      {
                        path: "history",
                        element: <DocumentHistoryPage />,
                      },
                      {
                        path: "ai-insights",
                        element: <DocumentAiInsightsPage />,
                      },
                    ],
                  },
                ],
              },
              // Organizations are regulatory parties (ADR-030, ADR-032) — the
              // sponsor, manufacturer or agent named on a submission — not
              // platform administration. They lived under /platform until
              // EPIC-016 S004, a leftover of the ADR-015 model where an
              // organization *was* the tenant. Tenants and Users stayed behind.
              // Correspondence is a tenant-wide list, a sibling of Products
              // and Organizations rather than a child of an application: the
              // question "what came in this week?" precedes knowing which
              // application a letter was about.
              // The epic's headline screen, and a sibling of everything else
              // under /regulatory: "what do I work on today?" is not a question
              // about one aggregate.
              {
                path: "due-work",
                element: <DueWorkPage />,
              },
              {
                path: "inspections",
                element: <InspectionsPage />,
              },
              // A sibling of Products, not a page inside a submission: a study
              // exists whether or not anything has been filed about it, and its
              // identity is the sponsor's (ADR-056).
              {
                path: "studies",
                element: <StudiesPage />,
              },
              // A sibling of Products, not a page beneath one: a substance is
              // a fact about the world that exists whether or not any product
              // contains it, which is what makes "which products contain
              // substance X?" askable backwards (ADR-058).
              {
                path: "substances",
                element: <SubstancesPage />,
              },
              {
                path: "meetings",
                element: <MeetingsPage />,
              },
              {
                path: "correspondence",
                children: [
                  {
                    index: true,
                    element: <CorrespondencePage />,
                  },
                  {
                    path: ":correspondenceId",
                    element: <CorrespondenceDetailPage />,
                  },
                ],
              },
              {
                path: "organizations",
                children: [
                  {
                    index: true,
                    element: <OrganizationsPage />,
                  },
                  // Organization Workspace — the first whose subject is a
                  // company rather than a product. Four angles on one party:
                  // who it is, how it is organised, where it operates, who it
                  // names.
                  {
                    path: ":organizationId",
                    element: <OrganizationWorkspaceLayout />,
                    children: [
                      {
                        index: true,
                        element: <OrganizationOverviewPage />,
                      },
                      {
                        path: "divisions",
                        element: <OrganizationDivisionsPage />,
                      },
                      {
                        path: "sites",
                        element: <OrganizationSitesPage />,
                      },
                      {
                        path: "contacts",
                        element: <OrganizationContactsPage />,
                      },
                    ],
                  },
                ],
              },
              // Tenant-wide directories, siblings rather than children. "Which
              // manufacturing sites do we have in India?" spans the registry,
              // and it is the question that made OrganizationSite and Contact
              // aggregate roots — the same reasoning that put Registrations
              // beside Products. Nesting them under /organizations would also
              // collide with :organizationId.
              {
                path: "sites",
                element: <SiteDirectoryPage />,
              },
              {
                path: "contacts",
                element: <ContactDirectoryPage />,
              },
              // Registrations are regulatory assets, not product work, so they
              // sit beside Products rather than beneath one. A registration has
              // a single canonical URL whichever portfolio axis reached it.
              {
                path: "registrations",
                children: [
                  {
                    index: true,
                    element: <RegistrationMarketsPage />,
                  },
                  {
                    path: "markets/:countryId",
                    element: <MarketRegistrationsPage />,
                  },
                  {
                    path: ":registrationId",
                    element: <RegistrationDetailPage />,
                  },
                ],
              },
            ],
          },
        ],
      },
    ],
  },
]);
