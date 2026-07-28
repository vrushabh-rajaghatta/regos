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
import { SubmissionWorkspaceLayout } from "@/features/regulatory/submissions/layout/SubmissionWorkspaceLayout";
import { SubmissionOverviewPage } from "@/features/regulatory/submissions/pages/SubmissionOverviewPage";
import { SubmissionDocumentsPage } from "@/features/regulatory/submissions/pages/SubmissionDocumentsPage";
import { SubmissionValidationPage } from "@/features/regulatory/submissions/pages/SubmissionValidationPage";
import { SubmissionPublishingPage } from "@/features/regulatory/submissions/pages/SubmissionPublishingPage";
import { SubmissionHistoryPage } from "@/features/regulatory/submissions/pages/SubmissionHistoryPage";
import { ProductDocumentsListPage } from "@/features/regulatory/documents/pages/ProductDocumentsListPage";
import { DocumentWorkspaceLayout } from "@/features/regulatory/documents/layout/DocumentWorkspaceLayout";
import { DocumentOverviewPage } from "@/features/regulatory/documents/pages/DocumentOverviewPage";
import { DocumentVersionsPage } from "@/features/regulatory/documents/pages/DocumentVersionsPage";
import { DocumentUsagePage } from "@/features/regulatory/documents/pages/DocumentUsagePage";
import { DocumentHistoryPage } from "@/features/regulatory/documents/pages/DocumentHistoryPage";
import { DocumentAiInsightsPage } from "@/features/regulatory/documents/pages/DocumentAiInsightsPage";
import { PlatformLayout } from "@/features/platform/layout/PlatformLayout";
import { OrganizationDetailsPage } from "@/features/platform/organizations/pages/OrganizationDetailsPage";
import { OrganizationsPage } from "@/features/platform/organizations/pages/OrganizationsPage";
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
                path: "organizations",
                element: <OrganizationsPage />,
              },
              {
                path: "organizations/:organizationId",
                element: <OrganizationDetailsPage />,
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
                    path: ":productId",
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
                    ],
                  },
                  // Application Workspace — execution-level view. Nested under the
                  // product URL, but a full-screen sibling so its sidebar replaces
                  // the product sidebar (rather than rendering two sidebars).
                  {
                    path: ":productId/applications/:applicationId",
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
                    path: ":productId/applications/:applicationId/submissions/:submissionId",
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
                        path: "validation",
                        element: <SubmissionValidationPage />,
                      },
                      {
                        path: "publishing",
                        element: <SubmissionPublishingPage />,
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
                    path: ":productId/documents/:documentId",
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
            ],
          },
        ],
      },
    ],
  },
]);
