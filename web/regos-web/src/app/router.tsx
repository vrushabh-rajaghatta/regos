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
import { UsersPage } from "@/features/platform/users/pages/UsersPage";
import { UserDetailsPage } from "@/features/platform/users/pages/UserDetailsPage";
import { Navigate } from "react-router-dom";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <AppLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: "platform",
        element: <PlatformLayout />,
        children: [
          {
            index: true,
            element: <Navigate to="users" replace />,
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
]);
