import { useQuery } from "@tanstack/react-query";

import { listRegistrationMarkets } from "../api/listRegistrationMarkets";

export function useRegistrationMarkets() {
  return useQuery({
    queryKey: ["registrations", "markets"],
    queryFn: listRegistrationMarkets,
  });
}
