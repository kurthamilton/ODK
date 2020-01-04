import { HttpResponse, HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { FormParameterEncoder } from './form-parameter-encoder';

export class HttpUtils {
  static createFormData(values: { [name: string]: string | File }): FormData {
    const formData = new FormData();

    for (const key in values) {
      if (values.hasOwnProperty(key)) {
        const value: any = values[key];
        if (typeof(value) === 'string') {
          formData.append(key, values[key]);
        } else {
          const file: File = <File>values[key];
          formData.append(key, file, file.name);
        }
      }
    }

    return formData;
  }

  static createFormParams(values: { [name: string]: string | string[] }): HttpParams {
    for (const key in values) {
      if (values.hasOwnProperty(key)) {
        values[key] = values[key] || ''
      }
    }

    return new HttpParams({
      fromObject: values,
      encoder: new FormParameterEncoder()
    });
  }

  static getBase64(http: HttpClient, url: string): Observable<string> {
    const request: Observable<HttpResponse<ArrayBuffer>> = http.get(url, { observe: 'response', responseType: 'arraybuffer' });
    return this.mapBase64Response(request);
  }

  static putBase64(http: HttpClient, url: string, body?: any): Observable<string> {
    const request: Observable<HttpResponse<ArrayBuffer>> = http.put(url, body || {}, { observe: 'response', responseType: 'arraybuffer' });
    return this.mapBase64Response(request);
  }

  private static mapBase64Response(request: Observable<HttpResponse<ArrayBuffer>>): Observable<string> {
    return request.pipe(
      map((response: HttpResponse<ArrayBuffer>) => {
        const contentType: string = response.headers.get('Content-Type');

        const bytes = new Uint8Array(response.body);
        const stringBytes: string = String.fromCharCode(...bytes)
        const base64: string = btoa(stringBytes);
        return base64 ? `data:${contentType};base64,${base64}` : '';
      })
    );
  }
}