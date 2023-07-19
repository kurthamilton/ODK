import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';

const baseUrl = `${environment.apiBaseUrl}/maps`;

const endpoints = {
  googleApiKey: (venueId: string) => `${baseUrl}/google/apikey${venueId ? `?venueId=${venueId}` : ''}`
};

@Injectable({
  providedIn: 'root'
})
export class MapService {

  constructor(private http: HttpClient) { }

  private apiKey: string;

  getGoogleMapsUrl(venueId: string, query: string): Observable<string> {
    return this.getApiKey(venueId).pipe(
      map((key: string) => `https://www.google.com/maps/embed/v1/place?key=${key}&q=${query}`)
    );
  }

  private getApiKey(venueId: string): Observable<string> {
    if (this.apiKey) {
      return of(this.apiKey);
    }

    return this.http.get(endpoints.googleApiKey(venueId)).pipe(
      map((response: any) => this.mapGoogleMapsApiKey(response))
    );
  }

  private mapGoogleMapsApiKey(response: any): string {
    return response.apiKey;
  }
}
