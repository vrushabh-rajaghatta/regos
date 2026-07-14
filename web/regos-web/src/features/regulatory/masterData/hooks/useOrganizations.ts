import { useQuery } from "@tanstack/react-query";
import { listOrganizations } from "../api/organizations";

export const useOrganizations = () => {
  return useQuery({
    queryKey: ["organizations"],
    queryFn: listOrganizations,
  });
};
