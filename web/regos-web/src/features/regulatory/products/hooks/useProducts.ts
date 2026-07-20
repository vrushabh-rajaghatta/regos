import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { listProducts, type ListProductsParams } from "../api/listProducts";

export const useProducts = (params: ListProductsParams = {}) => {
  return useQuery({
    queryKey: ["products", params],
    queryFn: () => listProducts(params),
    // Keeps the current page visible while the next one loads, so paging and
    // typing in the search box do not flash the loading state.
    placeholderData: keepPreviousData,
  });
};
