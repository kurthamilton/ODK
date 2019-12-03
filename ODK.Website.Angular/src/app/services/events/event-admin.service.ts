import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { Event } from 'src/app/core/events/event';
import { EventInvites } from 'src/app/core/events/event-invites';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { HttpUtils } from 'src/app/services/http/http-utils';
import { ServiceResult } from 'src/app/services/service-result';

const baseUrl = `${environment.baseUrl}/admin/events`

const endpoints = {
  chapterInvites: (chapterId: string) => `${baseUrl}/invites?chapterId=${chapterId}`,
  chapterResponses: (chapterId: string) => `${baseUrl}/responses?chapterId=${chapterId}`,
  createEvent: `${baseUrl}`,
  event: (id: string) => `${baseUrl}/${id}`,
  eventInvites: (eventId: string) => `${baseUrl}/${eventId}/invites`,
  eventResponses: (eventId: string) => `${baseUrl}/${eventId}/responses`,
  events: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  sendInvites: (eventId: string) => `${baseUrl}/${eventId}/sendinvites`
};

@Injectable({
  providedIn: 'root'
})
export class EventAdminService {

  constructor(private http: HttpClient) { }

  createEvent(event: Event): Observable<ServiceResult<Event>> {
    const params: HttpParams = this.createEventParams(event);

    return this.http.post(endpoints.createEvent, params).pipe(
      map((response: any): ServiceResult<Event> => ({
        success: true,
        value: this.mapEvent(response)
      })),
      catchError((err: any): Observable<ServiceResult<Event>> => {
        const response = err.error;
        const result: ServiceResult<Event> = {
          messages: response.messages,
          success: false
        };
        return of(result);
      })
    );
  }

  getChapterInvites(chapterId: string): Observable<EventInvites[]> {
    return this.http.get(endpoints.chapterInvites(chapterId)).pipe(
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

  getEvents(chapterId: string): Observable<Event[]> {
    return this.http.get(endpoints.events(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapEvent(x)))
    );
  }

  sendInvites(eventId: string): Observable<{}> {
    return this.http.post(endpoints.sendInvites(eventId), null).pipe(
      map(() => ({}))
    );
  }

  updateEvent(event: Event): Observable<ServiceResult<Event>> {
    const params: HttpParams = this.createEventParams(event);
    return this.http.put(endpoints.event(event.id), params).pipe(
      map((response: any) => ({
        success: true,
        value: this.mapEvent(response)
      })),
      catchError((err: any): Observable<ServiceResult<Event>> => {
        const response = err.error;
        const result: ServiceResult<Event> = {
          messages: response.messages,
          success: false
        };
        return of(result);
      })
    );
  }

  private createEventParams(event: Event): HttpParams {
    return HttpUtils.createFormParams({
      address: event.address,
      chapterId: event.chapterId,
      date: event.date.toISOString(),
      description: event.description,
      isPublic: event.isPublic ? 'true' : 'false',
      location: event.location,
      mapQuery: event.mapQuery,
      name: event.name,
      time: event.time
    });
  }

  private mapEvent(response: any): Event {
    return {
      address: response.address,
      chapterId: response.chapterId,
      date: new Date(response.date),
      description: response.description,
      id: response.id,
      imageUrl: response.imageUrl,
      isPublic: response.isPublic === true,
      location: response.location,
      mapQuery: response.mapQuery,
      name: response.name,
      time: response.time
    };
  }

  private mapEventInvites(response: any, id?: string): EventInvites {
    return {
      delivered: response ? response.delivered : 0,
      eventId: response ? response.eventId : id,
      sent: response ? response.sent : 0
    };
  }

  private mapEventMemberResponse(response: any): EventMemberResponse {
    return {
      eventId: response.eventId,
      memberId: response.memberId,
      responseType: response.responseTypeId
    };
  }
}
