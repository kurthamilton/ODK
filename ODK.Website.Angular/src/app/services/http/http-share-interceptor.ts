import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { shareReplay, finalize } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HttpShareInterceptor implements HttpInterceptor {

  private requests = {};
  
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return this.requests[request.url] || (this.requests[request.url] = next.handle(request).pipe(
      shareReplay(1),
      finalize(() => delete this.requests[request.url])
    ));      
  }
}
