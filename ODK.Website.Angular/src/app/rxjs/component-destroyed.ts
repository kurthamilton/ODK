import { OnDestroy } from '@angular/core';

import { ReplaySubject, Subject, Observable } from 'rxjs';

export function componentDestroyed(component: OnDestroy): Observable<void> {
  const oldNgOnDestroy: () => void = component.ngOnDestroy;
  const destroyed: Subject<void> = new ReplaySubject<void>(1);
  component.ngOnDestroy = () => {
    oldNgOnDestroy.apply(component);
    destroyed.next(undefined);
    destroyed.complete();
  };
  return destroyed.asObservable();
}
