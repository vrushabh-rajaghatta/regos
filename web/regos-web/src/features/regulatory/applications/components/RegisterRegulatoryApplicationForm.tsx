import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

import { useCreateRegulatoryApplication } from "../hooks/useCreateRegulatoryApplication";

interface Props {
  productId: string;
  onSuccess: () => void;
}

const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

export function RegisterRegulatoryApplicationForm({
  productId,
  onSuccess,
}: Props) {
  const [name, setName] = useState("");

  const mutation =
    useCreateRegulatoryApplication(productId);

  async function handleSubmit(
    e: React.FormEvent<HTMLFormElement>
  ) {
    e.preventDefault();

    await mutation.mutateAsync({
      name,

      authorityId: EMPTY_GUID,

      countryId: EMPTY_GUID,

      applicantOrganizationId: EMPTY_GUID,
    });

    onSuccess();
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="space-y-4"
    >
      <Input
        placeholder="Application name"
        value={name}
        onChange={(e) => setName(e.target.value)}
      />

      <Button
        type="submit"
        disabled={mutation.isPending}
      >
        Create Application
      </Button>
    </form>
  );
}
