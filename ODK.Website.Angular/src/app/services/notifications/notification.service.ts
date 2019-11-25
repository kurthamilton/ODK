import { Injectable } from '@angular/core';

import { Subject, Observable } from 'rxjs';

import { Notification } from 'src/app/core/notifications/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  private notificationsSubject: Subject<Notification> = new Subject<Notification>();

  publish(notification: Notification): void {
    if (!notification) {
      return;
    }

    this.notificationsSubject.next(notification);
  }

  subscribe(): Observable<Notification> {
    return this.notificationsSubject.asObservable();
  }
}
