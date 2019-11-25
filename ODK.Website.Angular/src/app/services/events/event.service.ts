import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AccountDetails } from 'src/app/core/account/account-details';
import { AuthenticationService } from '../authentication/authentication.service';
import { environment } from 'src/environments/environment';
import { Event } from 'src/app/core/events/event';

const baseUrl = `${environment.baseUrl}/events`;

const endpoints = {
  events: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  publicEvents: (chapterId: string) => `${baseUrl}/public?chapterId=${chapterId}`
}

@Injectable({
  providedIn: 'root'
})
export class EventService {

  constructor(private http: HttpClient,
    private authenticationService: AuthenticationService
  ) {     
  }

  getEvent(id: string, chapterId: string): Observable<Event> {
    return this.getEvents(chapterId).pipe(
      map((events: Event[]) => events.find(x => x.id === id))
    );
  }

  getEvents(chapterId: string): Observable<Event[]> {
    const accountDetails: AccountDetails = this.authenticationService.getAccountDetails();
    if (!accountDetails || accountDetails.chapterId !== chapterId) {
      return this.getPublicEvents(chapterId);
    }

    return this.http.get(endpoints.events(chapterId))
      .pipe(
        map((response: any) => response.map(x => this.mapEvent(x)))
      )
  }

  getPublicEvents(chapterId: string): Observable<Event[]> {
    return this.http.get(endpoints.publicEvents(chapterId))
      .pipe(
        map((response: any) => response.map(x => this.mapEvent(x)))
      )
  }

  private mapEvent(response: any): Event {
    return {
      address: response.address,
      chapterId: response.chapterId,
      date: new Date(response.date),
      description: response.description,
      id: response.id,
      imageUrl: response.imageUrl,
      isPublic: response.isPublic,
      location: response.location,
      mapQuery: response.mapQuery,
      name: response.name,
      time: response.time
    };
  }
}
