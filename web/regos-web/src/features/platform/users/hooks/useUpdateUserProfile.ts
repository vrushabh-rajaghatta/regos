import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  updateUserProfile,
  type UpdateUserProfileRequest,
} from "../api/updateUserProfile";

export function useUpdateUserProfile(userId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateUserProfileRequest) =>
      updateUserProfile(userId, request),

    onSuccess: () => {
      // Refresh both the directory and this user's details.
      queryClient.invalidateQueries({ queryKey: ["users"] });
    },
  });
}
