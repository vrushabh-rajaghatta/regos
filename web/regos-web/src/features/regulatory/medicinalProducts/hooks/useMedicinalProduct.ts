import { useQuery } from "@tanstack/react-query";

import { getMedicinalProduct } from "../api/getMedicinalProduct";

export function useMedicinalProduct(medicinalProductId: string) {
  return useQuery({
    queryKey: ["medicinal-products", "detail", medicinalProductId],
    queryFn: () => getMedicinalProduct(medicinalProductId),
    enabled: medicinalProductId !== "",
  });
}
