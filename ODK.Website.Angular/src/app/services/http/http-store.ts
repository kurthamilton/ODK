import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HttpStore {
  constructor(private http: HttpClient) {
  }

  private cache = {};

  get<T>(url: string, mapper: (response: any) => T): Observable<T> {
    if (this.cache.hasOwnProperty(url)) {
      return of(this.cache[url]);
    }

    return this.http.get(url).pipe(
      map((response: any) => mapper(response)),
      tap(value => this.cache[url] = value)
    );
  }
}