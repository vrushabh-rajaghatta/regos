import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

import { useResolveQuestion } from "../hooks/useResolveQuestion";
import { useRespondToQuestion } from "../hooks/useRespondToQuestion";
import type { CorrespondenceQuestionSummary } from "../types/CorrespondenceDetail";
import { QuestionHistoryTimeline } from "./QuestionHistoryTimeline";
import { RaiseQuestionDialog } from "./RaiseQuestionDialog";

interface CorrespondenceQuestionsProps {
  correspondenceId: string;
  questions: CorrespondenceQuestionSummary[];
}

/**
 * The questions inside a letter, each with its own history.
 *
 * Every capability this section adds is read back on the same page — the
 * history it writes is visible immediately below the question that wrote it
 * (testing.md principle 8, after EPIC-017 S003 shipped a history nobody could
 * see).
 */
export function CorrespondenceQuestions({
  correspondenceId,
  questions,
}: CorrespondenceQuestionsProps) {
  const [raising, setRaising] = useState(false);

  return (
    <section
      className="mt-6 rounded-lg border p-6"
      data-testid="correspondence-questions"
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold">Questions</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            What the authority asked, and what we said back.
          </p>
        </div>

        <Button onClick={() => setRaising(true)}>Raise question</Button>
      </div>

      {questions.length === 0 ? (
        <p
          className="mt-4 text-sm text-muted-foreground"
          data-testid="correspondence-questions-empty"
        >
          No questions raised from this letter yet.
        </p>
      ) : (
        <ul className="mt-4 divide-y">
          {questions.map((question) => (
            <QuestionRow
              key={question.questionId}
              correspondenceId={correspondenceId}
              question={question}
            />
          ))}
        </ul>
      )}

      <RaiseQuestionDialog
        correspondenceId={correspondenceId}
        open={raising}
        onOpenChange={setRaising}
      />
    </section>
  );
}

function QuestionRow({
  correspondenceId,
  question,
}: {
  correspondenceId: string;
  question: CorrespondenceQuestionSummary;
}) {
  const [responseText, setResponseText] = useState("");
  const [occurredOn, setOccurredOn] = useState("");

  const respond = useRespondToQuestion(correspondenceId, question.questionId);
  const resolve = useResolveQuestion(correspondenceId, question.questionId);

  const isResolved = question.currentStatus === "Resolved";

  return (
    <li className="py-4" data-testid="correspondence-question">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="font-medium">
            {question.number}. {question.text}
          </p>
          <p className="mt-1 text-sm text-muted-foreground">
            {question.currentStatus}
            {question.targetResponseOn
              ? ` · target ${question.targetResponseOn}`
              : ""}
            {question.respondedOn ? ` · answered ${question.respondedOn}` : ""}
          </p>
        </div>
      </div>

      {question.responseText && (
        <p className="mt-2 rounded-md bg-muted/40 p-3 text-sm">
          {question.responseText}
        </p>
      )}

      <QuestionHistoryTimeline history={question.history} />

      {!isResolved && (
        <div className="mt-3 space-y-2">
          {question.currentStatus === "Open" && (
            <div className="flex flex-wrap items-end gap-2">
              <label className="text-sm" htmlFor={`response-${question.questionId}`}>
                Our answer
                <textarea
                  id={`response-${question.questionId}`}
                  rows={2}
                  className="mt-1 w-80 rounded-md border bg-transparent p-2 text-sm"
                  value={responseText}
                  onChange={(event) => setResponseText(event.target.value)}
                />
              </label>

              <label className="text-sm" htmlFor={`sent-${question.questionId}`}>
                Sent on
                <Input
                  id={`sent-${question.questionId}`}
                  type="date"
                  className="mt-1"
                  value={occurredOn}
                  onChange={(event) => setOccurredOn(event.target.value)}
                />
              </label>

              <Button
                type="button"
                disabled={respond.isPending || !responseText || !occurredOn}
                onClick={() =>
                  respond
                    .mutateAsync({ responseText, occurredOn })
                    .catch(() => undefined)
                }
              >
                Record answer
              </Button>
            </div>
          )}

          {question.currentStatus === "Responded" && (
            <div className="flex flex-wrap items-end gap-2">
              <label className="text-sm" htmlFor={`resolved-${question.questionId}`}>
                Accepted on
                <Input
                  id={`resolved-${question.questionId}`}
                  type="date"
                  className="mt-1"
                  value={occurredOn}
                  onChange={(event) => setOccurredOn(event.target.value)}
                />
              </label>

              <Button
                type="button"
                variant="outline"
                disabled={resolve.isPending || !occurredOn}
                onClick={() =>
                  resolve.mutateAsync(occurredOn).catch(() => undefined)
                }
              >
                Mark resolved
              </Button>
            </div>
          )}
        </div>
      )}

      {(respond.isError || resolve.isError) && (
        <p
          className="mt-2 text-sm text-destructive"
          data-testid="question-action-error"
        >
          {((respond.error ?? resolve.error) as Error).message}
        </p>
      )}
    </li>
  );
}
