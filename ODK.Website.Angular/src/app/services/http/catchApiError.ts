import { OperatorFunction, Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ServiceResult } from '../service-result';

export function catchApiError<T>(value?: T): OperatorFunction<ServiceResult<T>, ServiceResult<any>> {
  return catchError((err: any): Observable<ServiceResult<T>> => {
    const response = err.error;
    const result: ServiceResult<T> = {
      messages: response.messages,
      success: false,
      value
    };
    return of(result);
  });
}
