import { RegulatoryLayout } from "@/features/regulatory/layout/RegulatoryLayout";
import { createBrowserRouter, Navigate } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AppLayout } from "@/shared/layout/AppLayout";
import { ProductListPage } from "@/features/regulatory/products/pages/ProductListPage";
import { ProductDetailsPage } from "@/features/regulatory/products/components/ProductDetailsPage";
import { ProductWorkspaceLayout } from "@/features/regulatory/products/layout/ProductWorkspaceLayout";
import { ProductOverviewPage } from "@/features/regulatory/products/pages/ProductOverviewPage";

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
                ],
              },
            ],
          },
        ],
      },
    ],
  },
]);
