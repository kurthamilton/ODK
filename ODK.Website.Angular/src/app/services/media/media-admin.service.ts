import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { MediaFile } from 'src/app/core/media/media-file';

const baseUrl: string = `${environment.adminApiBaseUrl}/media`;

const endpoints = {
  getMedia: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`
};

@Injectable({
  providedIn: 'root'
})
export class MediaAdminService {

  constructor(private http: HttpClient) { }

  getMedia(chapterId: string): Observable<MediaFile[]> {
    return this.http.get(endpoints.getMedia(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMediaFile(x)))
    );
  }

  private mapMediaFile(response: any): MediaFile {
    return {
      name: response.name,
      url: response.url
    };
  }
}
