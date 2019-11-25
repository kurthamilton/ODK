import { HttpEvent, HttpRequest, HttpErrorResponse } from '@angular/common/http';

import { Subject, Observable, ReplaySubject } from 'rxjs';

export class PendingRequestsQueue {

  private pendingRequests: Map<string, Subject<HttpEvent<any>>> = new Map<string, Subject<HttpEvent<any>>>();

  abort(request: HttpRequest<any>): void {
    this.finalize(request);
  }

  clear(): void {
    this.pendingRequests.forEach((subject: Subject<HttpEvent<any>>) => {
      subject.complete();
    });

    this.pendingRequests.clear();
  }

  complete(request: HttpRequest<any>, response: HttpEvent<any>): void {
    this.finalize(request, x => x.next(response));
  }

  enqueue(request: HttpRequest<any>): Observable<HttpEvent<any>> {
    return this.get(request) || this.add(request);
  }

  error(request: HttpRequest<any>, err: HttpErrorResponse): void {
    this.finalize(request, x => x.error(err));
  }

  get(request: HttpRequest<any>): Observable<HttpEvent<any>> {
    const subject: Subject<HttpEvent<any>> = this.getSubject(request);
    return subject ? subject.asObservable() : null;
  }

  private add(request: HttpRequest<any>): Observable<HttpEvent<any>> {
    const subject: Subject<HttpEvent<any>> = new ReplaySubject<HttpEvent<any>>(1);
    const url: string = this.getUrl(request);
    this.pendingRequests.set(url, subject);
    return subject.asObservable();
  }

  private finalize(request: HttpRequest<any>, action?: (subject: Subject<HttpEvent<any>>) => void): void {
    const url: string = this.getUrl(request);
    const subject: Subject<HttpEvent<any>> = this.pendingRequests.get(url);
    if (!subject) {
      return;
    }

    this.pendingRequests.delete(url);

    if (action) {
      action(subject);
    }

    subject.complete();
  }

  private getSubject(request: HttpRequest<any>): Subject<HttpEvent<any>> {
    const url: string = this.getUrl(request);
    return this.pendingRequests.get(url);
  }

  private getUrl(request: HttpRequest<any>): string {
    return request.urlWithParams;
  }
}
