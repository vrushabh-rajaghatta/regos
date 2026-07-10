import { useQuery } from "@tanstack/react-query";
import { listProducts } from "../api/listProducts";

export const useProducts = () => {
  return useQuery({
    queryKey: ["products"],
    queryFn: listProducts,
  });
};
