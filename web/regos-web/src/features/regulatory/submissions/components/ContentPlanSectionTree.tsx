import { Button } from "@/components/ui/button";

import type { ContentPlanSection } from "../types/SubmissionContentPlan";

interface Props {
  sections: ContentPlanSection[];
  /** Only a draft dossier may be reorganised — mirrors the aggregate. */
  editable: boolean;
  onPlace: (section: { id: string; label: string }) => void;
  /**
   * Takes a document out of the structure without detaching it. Misfiling has
   * to be reversible in the same place it happens, or the only way back is the
   * API.
   */
  onRemove: (submissionDocumentId: string) => void;
  /**
   * Names the study a placement reports. Offered on every placed document
   * rather than only in CTD 4.2.x and 5.3.x: which sections owe a study is a
   * regulatory rule, and deriving it here would mean the browser holding a
   * second copy of it. The generator refuses a missing one by name.
   */
  onReportStudy: (placement: {
    submissionDocumentId: string;
    documentName: string;
    studyId: string | null;
    fileTag: string | null;
  }) => void;
  depth?: number;
}

/**
 * The dossier as the blueprint's tree of placeholders.
 *
 * Renders the content plan and nothing else: whether a placeholder is
 * satisfied, what counts as supporting content, and how full the dossier is are
 * all decided by the server. Validation findings live on the Validation tab —
 * mapping issues back onto placeholders here would mean re-deriving dossier
 * semantics in the browser, and letting them disagree.
 */
export function ContentPlanSectionTree({
  sections,
  editable,
  onPlace,
  onRemove,
  onReportStudy,
  depth = 0,
}: Props) {
  return (
    <ul className={depth === 0 ? "space-y-4" : "mt-2 space-y-3 border-l pl-4"}>
      {sections.map((section) => (
        <li key={section.sectionId} data-testid="content-plan-section">
          <div className="flex items-baseline gap-2">
            <span className="font-mono text-sm text-muted-foreground">
              {section.code}
            </span>
            <span className={depth === 0 ? "font-semibold" : "font-medium"}>
              {section.title}
            </span>
          </div>

          {section.placeholders.length > 0 && (
            <ul className="mt-1 space-y-1">
              {section.placeholders.map((placeholder) => (
                <li
                  key={placeholder.placeholderId}
                  data-testid="content-plan-placeholder"
                  data-satisfied={placeholder.isSatisfied}
                  data-document-type={placeholder.documentTypeName}
                  // A grid, not a wrapping flex row: the marker keeps its own
                  // column so a long document-type name never leaves it
                  // stranded on a line of its own.
                  className="grid grid-cols-[auto_1fr] items-baseline gap-x-2 text-sm"
                >
                  <span aria-hidden="true">
                    {placeholder.isSatisfied ? "☑" : "☐"}
                  </span>

                  <span className="flex flex-wrap items-center gap-x-2 gap-y-1">
                    <span className="sr-only">
                      {placeholder.isSatisfied ? "Filled" : "Empty"}
                    </span>

                    <span
                      className={
                        placeholder.isSatisfied ? "" : "text-muted-foreground"
                      }
                    >
                      {placeholder.documentTypeName}
                    </span>

                    {!placeholder.isMandatory && (
                      <span className="text-xs text-muted-foreground">
                        Optional
                      </span>
                    )}

                    {placeholder.documents.map((document) => (
                      <span
                        key={document.submissionDocumentId}
                        data-testid="placeholder-document"
                        className="flex items-center gap-1"
                      >
                        <span className="font-mono text-xs text-muted-foreground">
                          {document.fileName}
                        </span>

                        {document.studyIdentifier && (
                          <span
                            data-testid="placement-study"
                            className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
                          >
                            {document.studyIdentifier}
                            {document.fileTag ? ` · ${document.fileTag}` : ""}
                          </span>
                        )}

                        {editable && (
                          <Button
                            size="sm"
                            variant="ghost"
                            data-testid="report-study"
                            aria-label={`Set the study ${document.name} reports in ${section.code} ${section.title}`}
                            onClick={() =>
                              onReportStudy({
                                submissionDocumentId:
                                  document.submissionDocumentId,
                                documentName: document.name,
                                studyId: document.studyId,
                                fileTag: document.fileTag,
                              })
                            }
                          >
                            {document.studyIdentifier ? "Study" : "Set study"}
                          </Button>
                        )}

                        {editable && (
                          <Button
                            size="sm"
                            variant="ghost"
                            data-testid="remove-placement"
                            aria-label={`Remove ${document.name} from ${section.code} ${section.title}`}
                            onClick={() =>
                              onRemove(document.submissionDocumentId)
                            }
                          >
                            Remove
                          </Button>
                        )}
                      </span>
                    ))}

                    {!placeholder.isSatisfied && editable && (
                      <Button
                        size="sm"
                        variant="outline"
                        data-testid="place-document"
                        onClick={() =>
                          onPlace({
                            id: section.sectionId,
                            label: `${section.code} ${section.title}`,
                          })
                        }
                      >
                        Place document
                      </Button>
                    )}
                  </span>
                </li>
              ))}
            </ul>
          )}

          {section.additionalDocuments.length > 0 && (
            <div
              data-testid="section-additional-documents"
              className="mt-1 text-sm text-muted-foreground"
            >
              {/* Content the blueprint does not require but the dossier
                  legitimately carries — listed, never flagged.

                  It carries the same per-document controls as a placeholder's
                  content, and that is not cosmetic: a study report's
                  supporting files sit in 4.2.x satisfying no placeholder, and
                  they need a study named on them exactly as the required ones
                  do. Rendered as a sentence, they were unreachable. */}
              <span>Also filed here:</span>

              <ul className="mt-1 space-y-1">
                {section.additionalDocuments.map((document) => (
                  <li
                    key={document.submissionDocumentId}
                    data-testid="additional-document"
                    className="flex flex-wrap items-center gap-x-2"
                  >
                    <span>{document.name}</span>

                    {document.studyIdentifier && (
                      <span
                        data-testid="placement-study"
                        className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
                      >
                        {document.studyIdentifier}
                        {document.fileTag ? ` · ${document.fileTag}` : ""}
                      </span>
                    )}

                    {editable && (
                      <Button
                        size="sm"
                        variant="ghost"
                        data-testid="report-study"
                        aria-label={`Set the study ${document.name} reports in ${section.code} ${section.title}`}
                        onClick={() =>
                          onReportStudy({
                            submissionDocumentId: document.submissionDocumentId,
                            documentName: document.name,
                            studyId: document.studyId,
                            fileTag: document.fileTag,
                          })
                        }
                      >
                        {document.studyIdentifier ? "Study" : "Set study"}
                      </Button>
                    )}

                    {editable && (
                      <Button
                        size="sm"
                        variant="ghost"
                        data-testid="remove-placement"
                        aria-label={`Remove ${document.name} from ${section.code} ${section.title}`}
                        onClick={() => onRemove(document.submissionDocumentId)}
                      >
                        Remove
                      </Button>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {section.children.length > 0 && (
            <ContentPlanSectionTree
              sections={section.children}
              editable={editable}
              onPlace={onPlace}
              onRemove={onRemove}
              onReportStudy={onReportStudy}
              depth={depth + 1}
            />
          )}
        </li>
      ))}
    </ul>
  );
}
