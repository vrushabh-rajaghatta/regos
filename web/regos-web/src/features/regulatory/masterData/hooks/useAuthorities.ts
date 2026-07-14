import { useQuery } from "@tanstack/react-query";
import { listAuthorities } from "../api/authorities";

export const useAuthorities = () => {
  return useQuery({
    queryKey: ["master-data", "authorities"],
    queryFn: listAuthorities,
  });
};
