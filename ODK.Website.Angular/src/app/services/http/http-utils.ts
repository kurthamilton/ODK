import { HttpResponse, HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { FormParameterEncoder } from './form-parameter-encoder';

export class HttpUtils {
    static createFormParams(values: { [name: string]: string }): HttpParams {
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
        return http.get(url, { observe: 'response', responseType: 'arraybuffer' })
            .pipe(
                map((response: HttpResponse<ArrayBuffer>) => {                    
                    const contentType: string = response.headers.get('Content-Type');

                    const bytes = new Uint8Array(response.body);
                    const stringBytes: string = String.fromCharCode(...bytes)                    
                    const base64: string = btoa(stringBytes);                    
                    return base64 ? `data:${contentType};base64,${base64}` : '';
                })
            )
    }
}