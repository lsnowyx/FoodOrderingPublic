export interface LocationSuggestion {
  readonly displayName: string;
  readonly latitude: number;
  readonly longitude: number;
  readonly type: string | null;
  readonly importance: number | null;
}
