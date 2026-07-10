import { RegulatoryLayout } from "@/features/regulatory/layout/RegulatoryLayout";
import { RegulatoryHomePage } from "@/features/regulatory/pages/RegulatoryHomePage";
import { createBrowserRouter, Navigate } from "react-router-dom";
import { HomePage } from "./pages/HomePage";
import { AppLayout } from "@/shared/layout/AppLayout";
import { ProductListPage } from "@/features/regulatory/products/pages/ProductListPage";

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
            index: true,
            element: <Navigate to="products" />, //<RegulatoryHomePage />,
          },
          {
            path: "products",
            element: <ProductListPage />,
          },
        ],
      },
    ],
  },
]);
