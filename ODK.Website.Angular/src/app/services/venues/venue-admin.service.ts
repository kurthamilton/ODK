import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { ServiceResult } from '../service-result';
import { Venue } from 'src/app/core/venues/venue';
import { VenueService } from './venue.service';

const baseUrl = `${environment.baseUrl}/admin/venues`;

const endpoints = {
  create: baseUrl,
  venue: (id: string) => `${baseUrl}/${id}`
};

@Injectable({
  providedIn: 'root'
})
export class VenueAdminService extends VenueService {

  constructor(http: HttpClient) {
    super(http);
  }

  createVenue(venue: Venue): Observable<ServiceResult<Venue>> {
    const params: HttpParams = this.createVenueParams(venue);

    return this.http.post(endpoints.create, params).pipe(
      map((response: any): ServiceResult<Venue> => ({
        success: true,
        value: this.mapVenue(response)
      })),
      catchError((err: any): Observable<ServiceResult<Venue>> => {
        const response = err.error;
        const result: ServiceResult<Venue> = {
          messages: response.messages,
          success: false
        };
        return of(result);
      })
    );
  }

  getVenue(id: string): Observable<Venue> {
    return this.http.get(endpoints.venue(id)).pipe(
      map((response: any) => this.mapVenue(response))
    );
  }

  updateVenue(venue: Venue): Observable<ServiceResult<Venue>> {
    const params: HttpParams = this.createVenueParams(venue);

    return this.http.put(endpoints.venue(venue.id), params).pipe(
      map((response: any) => ({
        success: true,
        value: this.mapVenue(response)
      })),
      catchError((err: any): Observable<ServiceResult<Venue>> => {
        const response = err.error;
        const result: ServiceResult<Venue> = {
          messages: response.messages,
          success: false
        };
        return of(result);
      })
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
}
