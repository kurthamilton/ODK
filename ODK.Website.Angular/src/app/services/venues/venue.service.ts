import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { Venue } from 'src/app/core/venues/venue';

const baseUrl = `${environment.baseUrl}/venues`;

const endpoints = {
  publicVenues: (chapterId: string) => `${baseUrl}/public?chapterId=${chapterId}`,
  venue: (id: string) => `${baseUrl}/${id}`,
  venues: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`
};

@Injectable({
  providedIn: 'root'
})
export class VenueService {

  constructor(protected http: HttpClient) { }

  getPublicVenues(chapterId: string): Observable<Venue[]> {
    return this.http.get(endpoints.publicVenues(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapVenue(x)))
    );
  }

  getVenue(id: string): Observable<Venue> {
    return this.http.get(endpoints.venue(id)).pipe(
      map((response: any) => this.mapVenue(response))
    );
  }

  getVenues(chapterId: string): Observable<Venue[]> {
    return this.http.get(endpoints.venues(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapVenue(x)))
    );
  }

  protected mapVenue(response: any): Venue {
    return {
      address: response.address,
      chapterId: response.chapterId,
      id: response.id,
      mapQuery: response.mapQuery,
      name: response.name
    };
  }
}
