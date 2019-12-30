import { Injectable } from '@angular/core';

import { Subject, Observable } from 'rxjs';

import { Notification } from 'src/app/core/notifications/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  
  private dismissedAlerts: string[] = [];
  private notificationsSubject: Subject<Notification> = new Subject<Notification>();

  alertIsDismissed(id: string): boolean {
    return this.dismissedAlerts.includes(id);
  }

  dismissAlert(id: string): void {
    this.dismissedAlerts.push(id);
  }

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
