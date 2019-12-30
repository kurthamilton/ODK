import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';

const baseUrl: string = `${environment.apiBaseUrl}/maps`;

const endpoints = {
  googleApiKey: `${baseUrl}/google/apikey`
};

@Injectable({
  providedIn: 'root'
})
export class MapService {

  constructor(private http: HttpClient) { }

  private apiKey: string;

  getGoogleMapsUrl(query: string): Observable<string> {
    return this.getApiKey().pipe(
      map((key: string) => `https://www.google.com/maps/embed/v1/place?key=${key}&q=${query}`)
    );
  }

  private getApiKey(): Observable<string> {
    if (this.apiKey) {
      return of(this.apiKey);
    }

    return this.http.get(endpoints.googleApiKey).pipe(
      map((response: any) => this.mapGoogleMapsApiKey(response))
    );
  }

  private mapGoogleMapsApiKey(response: any): string {
    return response.apiKey;
  }
}
