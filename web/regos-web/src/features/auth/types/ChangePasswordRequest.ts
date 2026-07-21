/**
 * Note what is absent: a user id. The API takes the caller's identity from
 * their token, and there is deliberately nowhere to name anyone else.
 */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
