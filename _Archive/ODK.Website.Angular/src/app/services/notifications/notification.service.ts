import { Injectable } from '@angular/core';

import { Subject, Observable, ReplaySubject, merge } from 'rxjs';
import { take } from 'rxjs/operators';

import { Notification } from 'src/app/core/notifications/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  private dismissedAlerts: string[] = [];
  private notificationsSubject: Subject<Notification> = new Subject<Notification>();
  private scheduled: Notification[] = [];

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

  schedule(notification: Notification): void {
    if (!notification) {
      return;
    }

    this.scheduled.push(notification);
  }

  subscribe(): Observable<Notification> {
    const replay: ReplaySubject<Notification> = new ReplaySubject<Notification>(this.scheduled.length);
    const scheduledCount = this.scheduled.length;
    if (scheduledCount > 0) {
      this.scheduled.forEach(x => {
        replay.next(x);
      });
      this.scheduled = [];
    }

    return merge(
      this.notificationsSubject.asObservable(),
      replay.pipe(
        take(scheduledCount)
      )
    );
  }
}
