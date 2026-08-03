import { Button } from "@/components/ui/button";

import { useGenerateSequencePackage } from "../hooks/useGenerateSequencePackage";

interface GenerateSequencePackageButtonProps {
  submissionId: string;
}

/**
 * The only wording this button is permitted.
 *
 * EPIC-007a's Definition of Done makes this a **product requirement, not a
 * documentation one**: a DTD-valid package with a wrong submission-type token
 * is perfectly legal XML that a gateway rejects, so structural validity is a
 * weaker promise than it sounds. RegOS reaches Level 2a — the package is
 * structurally legal, checked by a third-party parser — and not Level 2b, which
 * is FDA's own business rules.
 *
 * Permitted: "Generate eCTD Package", "Download Generated Package".
 * Forbidden: "FDA-ready", "Validated", "Ready for submission".
 *
 * Each forbidden phrase asserts a level of evidence this product does not
 * reach, and a browser spec asserts their absence rather than trusting review.
 */
export function GenerateSequencePackageButton({
  submissionId,
}: GenerateSequencePackageButtonProps) {
  const mutation = useGenerateSequencePackage(submissionId);

  return (
    <div className="flex flex-col items-end gap-1">
      <Button
        variant="outline"
        size="sm"
        data-testid="generate-package"
        disabled={mutation.isPending}
        onClick={() => mutation.mutate()}
      >
        {mutation.isPending ? "Generating..." : "Generate eCTD Package"}
      </Button>

      {mutation.isError && (
        <p
          className="max-w-md text-right text-sm text-destructive"
          role="alert"
          data-testid="generate-package-error"
        >
          {mutation.error.message}
        </p>
      )}
    </div>
  );
}
