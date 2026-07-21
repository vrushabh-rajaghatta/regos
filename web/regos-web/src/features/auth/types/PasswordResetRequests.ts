export interface RequestPasswordResetRequest {
  email: string;
}

export interface CompletePasswordResetRequest {
  token: string;
  password: string;
}
