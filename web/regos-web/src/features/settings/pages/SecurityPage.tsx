import { ChangePasswordForm } from "@/features/auth/components/ChangePasswordForm";
import { PageHeader } from "@/shared/components/PageHeader";

export function SecurityPage() {
  return (
    <div className="space-y-8">
      <PageHeader
        title="Security"
        description="How you sign in to RegOS."
      />

      <ChangePasswordForm />
    </div>
  );
}
