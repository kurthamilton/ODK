import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AuthenticationService } from '../authentication/authentication.service';
import { catchApiError } from '../http/catchApiError';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { ServiceResult } from '../service-result';
import { Venue } from 'src/app/core/venues/venue';
import { VenueService } from './venue.service';
import { VenueStats } from 'src/app/core/venues/venue-stats';

const baseUrl = `${environment.apiBaseUrl}/admin/venues`;

const endpoints = {
  chapterStats: (chapterId: string) => `${baseUrl}/stats?chapterId=${chapterId}`,
  create: baseUrl,
  venue: (id: string) => `${baseUrl}/${id}`
};

@Injectable({
  providedIn: 'root'
})
export class VenueAdminService extends VenueService {

  constructor(http: HttpClient,
    authenticationService: AuthenticationService
  ) {
    super(http, authenticationService);
  }

  private activeVenue: Venue;

  createVenue(venue: Venue): Observable<ServiceResult<Venue>> {
    const params: HttpParams = this.createVenueParams(venue);

    return this.http.post(endpoints.create, params).pipe(
      map((response: any): ServiceResult<Venue> => ({
        success: true,
        value: this.mapVenue(response)
      })),
      catchApiError()
    );
  }

  getActiveVenue(): Venue {
    return this.activeVenue;
  }
  
  getChapterStats(chapterId: string): Observable<VenueStats[]> {
    return this.http.get(endpoints.chapterStats(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapVenueStats(x)))
    );
  }

  getVenue(id: string): Observable<Venue> {
    return this.http.get(endpoints.venue(id)).pipe(
      map((response: any) => this.mapVenue(response))
    );
  }

  setActiveVenue(venue: Venue): void {
    this.activeVenue = venue;
  }

  updateVenue(venue: Venue): Observable<ServiceResult<Venue>> {
    const params: HttpParams = this.createVenueParams(venue);

    return this.http.put(endpoints.venue(venue.id), params).pipe(
      map((response: any) => ({
        success: true,
        value: this.mapVenue(response)
      })),
      catchApiError()
    );
  }

  private createVenueParams(venue: Venue): HttpParams {
    return HttpUtils.createFormParams({
      address: venue.address,
      chapterId: venue.chapterId,
      mapQuery: venue.mapQuery,
      name: venue.name
    });
  }

  private mapVenueStats(response: any): VenueStats {
    return {
      eventCount: response.eventCount,
      lastEventDate: response.lastEventDate ? new Date(response.lastEventDate) : null,
      venueId: response.venueId
    };
  }
}
