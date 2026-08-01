import { useQuery } from "@tanstack/react-query";

import { listMedicinalProducts } from "../api/listMedicinalProducts";

export function useMedicinalProducts(globalProductId: string) {
  return useQuery({
    queryKey: ["medicinal-products", "product", globalProductId],
    queryFn: () => listMedicinalProducts(globalProductId),
    enabled: globalProductId !== "",
  });
}
