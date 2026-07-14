import { useQuery } from "@tanstack/react-query";
import { listCountries } from "../api/countries";

export const useCountries = () => {
  return useQuery({
    queryKey: ["master-data", "countries"],
    queryFn: listCountries,
  });
};
