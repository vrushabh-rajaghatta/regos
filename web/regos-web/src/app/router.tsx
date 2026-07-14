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
import { SubmissionPublishingPage } from "@/features/regulatory/submissions/pages/SubmissionPublishingPage";
import { SubmissionHistoryPage } from "@/features/regulatory/submissions/pages/SubmissionHistoryPage";

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
                    path: "publishing",
                    element: <SubmissionPublishingPage />,
                  },
                  {
                    path: "history",
                    element: <SubmissionHistoryPage />,
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
