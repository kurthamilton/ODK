import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AuthenticationService } from '../authentication/authentication.service';
import { catchApiError } from '../http/catchApiError';
import { environment } from 'src/environments/environment';
import { Event } from 'src/app/core/events/event';
import { EventInvites } from 'src/app/core/events/event-invites';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventService } from './event.service';
import { HttpUtils } from 'src/app/services/http/http-utils';
import { ServiceResult } from 'src/app/services/service-result';

const baseUrl = `${environment.baseUrl}/admin/events`

const endpoints = {
  chapterInvites: (chapterId: string, page: number, pageCount: number) => 
    `${baseUrl}/invites?chapterId=${chapterId}&page=${page}&pageCount=${pageCount}`,
  chapterResponses: (chapterId: string) => `${baseUrl}/responses?chapterId=${chapterId}`,
  count: (chapterId: string) => `${baseUrl}/count?chapterId=${chapterId}`,
  createEvent: `${baseUrl}`,
  event: (id: string) => `${baseUrl}/${id}`,
  eventInvites: (eventId: string) => `${baseUrl}/${eventId}/invites`,
  eventResponses: (eventId: string) => `${baseUrl}/${eventId}/responses`,
  events: (chapterId: string, page: number, pageCount: number) => 
    `${baseUrl}?chapterId=${chapterId}&page=${page}&pageCount=${pageCount}`,
  eventsByVenue: (venueId: string) => `${baseUrl}/venues/${venueId}`,
  sendInvites: (eventId: string, test?: boolean) => `${baseUrl}/${eventId}/invites/send${test ? '/test' : ''}`
};

@Injectable({
  providedIn: 'root'
})
export class EventAdminService extends EventService {

  constructor(http: HttpClient,
    authenticationService: AuthenticationService
  ) {
    super(http, authenticationService);
  }

  private activeEvent: Event;

  createEvent(event: Event): Observable<ServiceResult<Event>> {
    const params: HttpParams = this.createEventParams(event);

    return this.http.post(endpoints.createEvent, params).pipe(
      map((response: any): ServiceResult<Event> => ({
        success: true,
        value: this.mapEvent(response)
      })),
      catchApiError()
    );
  }

  deleteEvent(id: string): Observable<void> {
    return this.http.delete(endpoints.event(id)).pipe(
      map(() => undefined)
    );
  }

  getActiveEvent(): Event {
    return this.activeEvent;
  }

  getAdminEvents(chapterId: string, page: number, pageCount: number): Observable<Event[]> {
    return this.http.get(endpoints.events(chapterId, page, pageCount)).pipe(
      map((response: any) => response.map(x => this.mapEvent(x)))
    );
  }
  
  getChapterInvites(chapterId: string, page: number, pageCount: number): Observable<EventInvites[]> {
    return this.http.get(endpoints.chapterInvites(chapterId, page, pageCount)).pipe(
      map((response: any) => response.map(x => this.mapEventInvites(x)))
    );
  }

  getChapterResponses(chapterId: string): Observable<EventMemberResponse[]> {
    return this.http.get(endpoints.chapterResponses(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapEventMemberResponse(x)))
    );
  }

  getEvent(id: string): Observable<Event> {
    return this.http.get(endpoints.event(id)).pipe(
      map((response: any) => this.mapEvent(response))
    );
  }

  getEventCount(chapterId: string): Observable<number> {
    return this.http.get(endpoints.count(chapterId)).pipe(
      map((response: any) => response)
    );
  }

  getEventInvites(eventId: string): Observable<EventInvites> {
    return this.http.get(endpoints.eventInvites(eventId)).pipe(
      map((response: any) => this.mapEventInvites(response, eventId))
    );
  }

  getEventResponses(eventId: string): Observable<EventMemberResponse[]> {
    return this.http.get(endpoints.eventResponses(eventId)).pipe(
      map((response: any) => response.map(x => this.mapEventMemberResponse(x)))
    );
  }  

  getEventsByVenue(venueId: string): Observable<Event[]> {
    return this.http.get(endpoints.eventsByVenue(venueId)).pipe(
      map((response: any) => response.map(x => this.mapEvent(x)))
    );
  }

  sendInvites(eventId: string, test?: boolean): Observable<{}> {
    return this.http.post(endpoints.sendInvites(eventId, test), null).pipe(
      map(() => ({}))
    );
  }

  setActiveEvent(event: Event): void {
    this.activeEvent = event;
  }

  updateEvent(event: Event): Observable<ServiceResult<Event>> {
    const params: HttpParams = this.createEventParams(event);
    return this.http.put(endpoints.event(event.id), params).pipe(
      map((response: any) => ({
        success: true,
        value: this.mapEvent(response)
      })),
      catchApiError()
    );
  }

  private createEventParams(event: Event): HttpParams {
    return HttpUtils.createFormParams({
      chapterId: event.chapterId,
      date: event.date.toISOString(),
      description: event.description,
      isPublic: event.isPublic ? 'true' : 'false',
      name: event.name,
      time: event.time,
      venueId: event.venueId
    });
  }

  private mapEventInvites(response: any, id?: string): EventInvites {
    return {
      eventId: response ? response.eventId : id,
      sent: response ? response.sent : 0,
      sentDate: (!!response && response.sentDate) ? new Date(response.sentDate) : null
    };
  }
}
