// Numeric values match the ElectronicLibrary.Domain.Enums ordinals — no JsonStringEnumConverter is
// configured server-side, so these come across the wire as plain numbers.
export enum AssetType {
  Score = 0,
  Diploma = 1,
  Poster = 2,
  Recording = 3
}

export enum AssetStatus {
  Pending = 0,
  Ready = 1
}

// labelKey points at a translation key (assets.types.<ordinal> in the i18n JSON files) — resolve it
// through the `translate` pipe/service, don't render it directly.
export const UPLOADABLE_ASSET_TYPES: { value: AssetType; labelKey: string }[] = [
  { value: AssetType.Score, labelKey: 'assets.types.0' },
  { value: AssetType.Diploma, labelKey: 'assets.types.1' },
  { value: AssetType.Poster, labelKey: 'assets.types.2' }
  // Recording is deferred per CLAUDE.md §10 — the Function doesn't process that prefix yet.
];

export const ALL_ASSET_TYPES: { value: AssetType; labelKey: string }[] = [
  ...UPLOADABLE_ASSET_TYPES,
  { value: AssetType.Recording, labelKey: 'assets.types.3' }
];

export function assetTypeLabelKey(type: AssetType): string {
  return `assets.types.${type}`;
}

export function assetStatusLabelKey(status: AssetStatus): string {
  return status === AssetStatus.Ready ? 'assets.status.ready' : 'assets.status.pending';
}

export interface AssetModel {
  id: string;
  type: AssetType;
  status: AssetStatus;
  blobName: string;
  title: string;
  contentType: string;
  sizeBytes: number;
  uploadedBy: string;
  thumbnailUrl: string | null;
}

export interface CreateAssetUploadSasRequest {
  fileName: string;
  contentType: string;
  type: AssetType;
}

export interface AssetUploadSasModel {
  uploadUrl: string;
  blobName: string;
}
