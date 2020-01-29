import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { MediaFile } from 'src/app/core/media/media-file';

const baseUrl: string = `${environment.adminApiBaseUrl}/media`;

const endpoints = {
  file: (chapterId: string, file: MediaFile) => `${baseUrl}/${file.name}?chapterId=${chapterId}`,
  files: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`
};

@Injectable({
  providedIn: 'root'
})
export class MediaAdminService {

  constructor(private http: HttpClient) { }

  deleteMediaFile(chapterId: string, file: MediaFile): Observable<MediaFile[]> {
    return this.http.delete(endpoints.file(chapterId, file)).pipe(
      map((response: any) => response.map(x => this.mapMediaFile(x)))
    );
  }

  getMediaFiles(chapterId: string): Observable<MediaFile[]> {
    return this.http.get(endpoints.files(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMediaFile(x)))
    );
  }

  uploadMediaFile(chapterId: string, file: File): Observable<MediaFile[]> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post(endpoints.files(chapterId), formData).pipe(
      map((response: any) => response.map(x => this.mapMediaFile(x)))
    );
  }

  private mapMediaFile(response: any): MediaFile {
    return {
      createdDate: new Date(response.date),
      name: response.name,
      url: response.url
    };
  }
}
