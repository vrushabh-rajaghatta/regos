import type { CodedValue } from "./Presentation";

/**
 * What the patient physically receives — a vial, a pen, the kit holding them.
 *
 * The list arrives flat, in reading order, each row saying what holds it and
 * how deep it sits. **The server sends `depth` rather than letting the client
 * derive it**: it already walked the tree to order the rows, and a second
 * implementation of the same walk is how two answers start to disagree.
 */
export interface Component {
  componentId: string;
  medicinalProductId: string;
  /** Null for what the patient is handed. */
  parentComponentId: string | null;
  /** One for a top-level article. */
  depth: number;
  componentType: CodedValue;
  name: string;
  description: string | null;
  quantity: number;
  unitOfPresentation: CodedValue | null;
  doseForm: CodedValue | null;
}

/** What add and restate both send. */
export interface ComponentBody {
  componentTypeCode: string;
  name: string;
  description: string | null;
  quantity: number;
  unitOfPresentationCode: string | null;
  doseFormCode: string | null;
}
