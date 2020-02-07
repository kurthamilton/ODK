import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { FileInfo } from '../../core/files/file-info';

@Injectable({
  providedIn: 'root'
})
export class FileService {

  constructor(private http: HttpClient) { }

  downloadFile(fileInfo: FileInfo): Observable<Blob> {
    const headers = new HttpHeaders();

    const mimeType: string = this.getMimeType(fileInfo.extension);
    if (mimeType) {
      headers.set('accept', mimeType);
    }

    return this.http
      .get(fileInfo.url, { headers, responseType: 'blob' })
      .pipe(
        map((response: any) => response)
      );
  }

  private getMimeType(extension: string) {
    switch (extension) {
      case 'csv':
        return 'text/csv';
    }
  }
}
