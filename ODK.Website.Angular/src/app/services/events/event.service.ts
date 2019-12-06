import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AccountDetails } from 'src/app/core/account/account-details';
import { AuthenticationService } from '../authentication/authentication.service';
import { environment } from 'src/environments/environment';
import { Event } from 'src/app/core/events/event';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';

const baseUrl = `${environment.baseUrl}/events`;

const endpoints = {
  eventResponses: (eventId: string) => `${baseUrl}/${eventId}/responses`,
  events: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  publicEvents: (chapterId: string) => `${baseUrl}/public?chapterId=${chapterId}`,
  respond: (eventId: string, type: number) => `${baseUrl}/${eventId}/respond?type=${type}`
}

@Injectable({
  providedIn: 'root'
})
export class EventService {

  constructor(protected http: HttpClient,
    private authenticationService: AuthenticationService
  ) {
  }

  getEvent(id: string, chapterId: string): Observable<Event> {
    return this.getEvents(chapterId).pipe(
      map((events: Event[]) => events.find(x => x.id === id))
    );
  }

  getEventResponses(eventId: string): Observable<EventMemberResponse[]> {
    return this.http.get(endpoints.eventResponses(eventId)).pipe(
      map((response: any) => response.map(x => this.mapEventMemberResponse(x)))
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

  respond(eventId: string, responseType: EventResponseType): Observable<EventMemberResponse> {
    return this.http.put(endpoints.respond(eventId, responseType), null).pipe(
      map((response: any) => this.mapEventMemberResponse(response))
    )
  }

  protected mapEvent(response: any): Event {
    return {
      chapterId: response.chapterId,
      date: new Date(response.date),
      description: response.description,
      id: response.id,
      imageUrl: response.imageUrl,
      isPublic: response.isPublic,
      name: response.name,
      time: response.time,
      venueId: response.venueId
    };
  }

  protected mapEventMemberResponse(response: any): EventMemberResponse {
    return {
      eventId: response.eventId,
      memberId: response.memberId,
      responseType: response.responseTypeId
    }
  }
}
