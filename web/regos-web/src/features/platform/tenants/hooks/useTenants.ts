import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  activateTenant,
  createTenant,
  deactivateTenant,
  listTenants,
} from "../api/tenantsApi";

export function useTenants() {
  return useQuery({
    queryKey: ["platform-tenants"],
    queryFn: listTenants,
  });
}

export function useCreateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["platform-tenants"] });
    },
  });
}

export function useActivateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: activateTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["platform-tenants"] });
    },
  });
}

export function useDeactivateTenant() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deactivateTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["platform-tenants"] });
    },
  });
}
