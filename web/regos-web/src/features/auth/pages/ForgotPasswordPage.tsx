import { ForgotPasswordForm } from "../components/ForgotPasswordForm";

export function ForgotPasswordPage() {
  return (
    <div className="min-h-screen flex items-center justify-center p-6">
      <div className="w-full max-w-sm space-y-8">
        <div>
          <h1 className="text-3xl font-semibold">Reset your password</h1>

          <p className="mt-2 text-muted-foreground">
            Enter your email address and we will send you a link to choose a new
            one.
          </p>
        </div>

        <ForgotPasswordForm />
      </div>
    </div>
  );
}
