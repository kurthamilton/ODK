import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { LogMessage } from 'src/app/core/logging/log-message';

const baseUrl: string = environment.adminApiBaseUrl;

const endpoints = {
  log: (level: string, page: number) => `${baseUrl}/log?level=${level ? level : ''}&page=${page ? page : ''}`
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  constructor(private http: HttpClient) { }
  
  getLogMessages(level?: string, page?: number): Observable<LogMessage[]> {
    return this.http.get(endpoints.log(level, page)).pipe(
      map((response: any) => response.map(x => this.mapLogMessage(x)))
    );
  }

  private mapLogMessage(response: any): LogMessage {
    return {
      exception: response.exception,
      level: response.level,
      message: response.message,
      timeStamp: new Date(response.timeStamp),
      type: response.type
    };
  }
}
