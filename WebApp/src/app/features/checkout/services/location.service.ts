import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { LocationSuggestion } from '../models/location-suggestion';

function parseLocationSuggestions(value: unknown): LocationSuggestion[] {
  if (!Array.isArray(value)) {
    throw new Error('Malformed location response.');
  }

  return value.map(item => {
    if (typeof item !== 'object' || item === null) {
      throw new Error('Malformed location response.');
    }

    const suggestion = item as Record<string, unknown>;
    const displayName = suggestion['displayName'];
    const latitude = suggestion['latitude'];
    const longitude = suggestion['longitude'];
    const type = suggestion['type'];
    const importance = suggestion['importance'];

    if (
      typeof displayName !== 'string' ||
      displayName.trim().length === 0 ||
      typeof latitude !== 'number' ||
      !Number.isFinite(latitude) ||
      latitude < -90 ||
      latitude > 90 ||
      typeof longitude !== 'number' ||
      !Number.isFinite(longitude) ||
      longitude < -180 ||
      longitude > 180 ||
      !(type === null || typeof type === 'string') ||
      !(importance === null || (typeof importance === 'number' && Number.isFinite(importance)))
    ) {
      throw new Error('Malformed location response.');
    }

    return { displayName, latitude, longitude, type, importance };
  });
}

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly http = inject(HttpClient);

  search(query: string, limit = 5): Observable<LocationSuggestion[]> {
    return this.http.get<unknown>('/api/locations/search', {
      params: { query, limit }
    }).pipe(map(parseLocationSuggestions));
  }
}
