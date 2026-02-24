export type MpaaRating = 'G' | 'PG' | 'PG-13' | 'R' | 'NC-17';

export interface FilmListItem {
  id: number;
  title: string;
  description: string | null;
  releaseYear: number | null;
  languageId: number;
  originalLanguageId: number | null;
  rentalDuration: number;
  rentalRate: number;
  length: number | null;
  replacementCost: number;
  rating: MpaaRating | null;
  specialFeatures: string[] | null;
  lastUpdate: string;
}

export interface FilmDetail extends FilmListItem {
  languageName: string;
  originalLanguageName: string | null;
  actors: string[];
  categories: string[];
}

export interface FilmSearchParams {
  search?: string;
  category?: string;
  rating?: MpaaRating;
  yearFrom?: number;
  yearTo?: number;
  page?: number;
  pageSize?: number;
}
