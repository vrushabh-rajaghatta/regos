import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  changeRegistrationStatus,
  createRegistration,
  getRegistration,
  listExpiringRegistrations,
  listMarketRegistrations,
  listProductRegistrations,
  listRegistrationMarkets,
  recordRegistrationApproval,
  type ChangeStatusBody,
  type CreateRegistrationBody,
  type RecordApprovalBody,
} from "../api/registrations";

export function useProductRegistrations(productId: string) {
  return useQuery({
    queryKey: ["registrations", "product", productId],
    queryFn: () => listProductRegistrations(productId),
    enabled: !!productId,
  });
}

export function useMarketRegistrations(countryId: string) {
  return useQuery({
    queryKey: ["registrations", "market", countryId],
    queryFn: () => listMarketRegistrations(countryId),
    enabled: !!countryId,
  });
}

export function useRegistrationMarkets() {
  return useQuery({
    queryKey: ["registrations", "markets"],
    queryFn: listRegistrationMarkets,
  });
}

export function useExpiringRegistrations() {
  return useQuery({
    queryKey: ["registrations", "expiring"],
    queryFn: listExpiringRegistrations,
  });
}

export function useRegistration(registrationId: string) {
  return useQuery({
    queryKey: ["registrations", registrationId],
    queryFn: () => getRegistration(registrationId),
    enabled: !!registrationId,
  });
}

export function useCreateRegistration(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: CreateRegistrationBody) =>
      createRegistration(productId, body),

    onSuccess: () => {
      // A new registration changes both portfolio axes and the market index —
      // a country we held nothing in a moment ago may now be on the list.
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}

/**
 * Recording the grant and changing the status are separate operations because
 * the grant establishes the number and validity dates. Both invalidate the same
 * keys: either one moves the registration through its lifecycle, and the
 * portfolio views show its status.
 */
export function useRecordRegistrationApproval(registrationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordApprovalBody) =>
      recordRegistrationApproval(registrationId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}

export function useChangeRegistrationStatus(registrationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ChangeStatusBody) =>
      changeRegistrationStatus(registrationId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}
