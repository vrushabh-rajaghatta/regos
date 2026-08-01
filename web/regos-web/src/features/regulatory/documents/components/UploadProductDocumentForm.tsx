import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useDocumentTypes } from "../hooks/useDocumentTypes";
import { useUploadProductDocument } from "../hooks/useUploadProductDocument";
import {
  uploadProductDocumentSchema,
  type UploadProductDocumentFormValues,
} from "../validation/uploadProductDocumentSchema";

interface Props {
  globalProductId: string;
  onSuccess: () => void;
}

export function UploadProductDocumentForm({ globalProductId, onSuccess }: Props) {
  const navigate = useNavigate();

  const documentTypesQuery = useDocumentTypes();
  const mutation = useUploadProductDocument(globalProductId);

  const [file, setFile] = useState<File | null>(null);
  const [fileError, setFileError] = useState<string | null>(null);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<UploadProductDocumentFormValues>({
    resolver: zodResolver(uploadProductDocumentSchema),
    defaultValues: {
      documentTypeId: "",
      name: "",
    },
  });

  const documentTypeItems = useMemo(
    () =>
      Object.fromEntries(
        (documentTypesQuery.data ?? []).map((type) => [
          type.id,
          `${type.name} (${type.code})`,
        ])
      ),
    [documentTypesQuery.data]
  );

  async function onSubmit(values: UploadProductDocumentFormValues) {
    if (!file) {
      setFileError("A file is required.");
      return;
    }

    const { id } = await mutation.mutateAsync({
      documentTypeId: values.documentTypeId,
      name: values.name,
      file,
    });

    reset();
    setFile(null);
    onSuccess();

    // Uploading is how a document is created — take the user straight into
    // the new document's workspace.
    navigate(`/regulatory/products/${globalProductId}/documents/${id}`);
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="documentTypeId"
          render={({ field }) => (
            <Field data-invalid={!!errors.documentTypeId}>
              <FieldLabel htmlFor="documentTypeId">Document Type</FieldLabel>

              <Select
                items={documentTypeItems}
                value={field.value}
                onValueChange={field.onChange}
              >
                <SelectTrigger id="documentTypeId" className="w-full">
                  <SelectValue placeholder="Select document type" />
                </SelectTrigger>

                <SelectContent>
                  {(documentTypesQuery.data ?? []).map((type) => (
                    <SelectItem key={type.id} value={type.id}>
                      {type.name} ({type.code})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.documentTypeId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Document Name</FieldLabel>

              <Input
                id="name"
                placeholder="e.g. Clinical Evaluation Report 2027"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Field data-invalid={!!fileError}>
          <FieldLabel htmlFor="file">File</FieldLabel>

          <input
            id="file"
            type="file"
            onChange={(event) => {
              setFile(event.target.files?.[0] ?? null);
              setFileError(null);
            }}
            className="block w-full text-sm text-muted-foreground file:mr-4 file:rounded-md file:border-0 file:bg-primary file:px-3 file:py-2 file:text-sm file:font-medium file:text-primary-foreground hover:file:bg-primary/90"
          />

          {fileError && (
            <p className="text-sm text-destructive">{fileError}</p>
          )}
        </Field>
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm font-normal text-destructive">
          {mutation.error.message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Uploading..." : "Upload Document"}
        </Button>
      </div>
    </form>
  );
}
