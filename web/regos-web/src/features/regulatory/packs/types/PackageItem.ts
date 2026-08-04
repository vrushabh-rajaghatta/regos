/**
 * One layer of a pack — the carton, the blisters inside it.
 *
 * **Not a component.** A component has a dose form and is part of what the
 * medicine *is*; a layer has a **material** and is how it is held (ADR-061 §1).
 *
 * `depth` is computed by the same tree the domain rules use, so the indentation
 * on screen and the depth the guard measured cannot drift apart.
 */
export interface PackageItem {
  id: string;
  parentPackageItemId: string | null;
  depth: number;
  itemTypeCode: string;
  itemTypeDisplay: string;
  itemTypeSystem: string;
  materialCode: string | null;
  materialDisplay: string | null;
  quantity: number;
  unitOfPresentationCode: string | null;
  unitOfPresentationDisplay: string | null;
  description: string | null;
}

export interface PackagingVocabulary {
  packageItemTypes: { system: string; code: string; display: string }[];
  materials: { system: string; code: string; display: string }[];
}
