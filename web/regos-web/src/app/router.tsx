import { RegulatoryLayout } from "@/features/regulatory/layout/RegulatoryLayout";
import { createBrowserRouter } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AppLayout } from "@/shared/layout/AppLayout";
import { ProductListPage } from "@/features/regulatory/products/pages/ProductListPage";
import { ProductWorkspaceLayout } from "@/features/regulatory/products/layout/ProductWorkspaceLayout";
import { ProductOverviewPage } from "@/features/regulatory/products/pages/ProductOverviewPage";
import { RegulatoryApplicationListPage } from "@/features/regulatory/applications/pages/RegulatoryApplicationListPage";
import { ApplicationOverviewPage } from "@/features/regulatory/applications/pages/ApplicationOverviewPage";

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
                      {
                        path: ":applicationId",
                        element: <ApplicationOverviewPage />,
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
