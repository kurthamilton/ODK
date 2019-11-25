import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { Notification } from 'src/app/core/notifications/notification';
import { NotificationService } from 'src/app/services/notifications/notification.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private notificationService: NotificationService
  ) { 
  }

  notifications: Notification[] = [];

  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnInit(): void {
    this.notificationService.subscribe()
      .pipe(
        takeUntil(this.destroyed)
      )
      .subscribe((notification: Notification) => {
        this.notifications.push(notification);
        this.changeDetector.detectChanges();
      });
  }

  ngOnDestroy(): void {
    this.destroyed.next({});
  }

  onDismiss(notification: Notification): void {
    const index: number = this.notifications.indexOf(notification);
    this.notifications.splice(index, 1);
    this.changeDetector.detectChanges();
  }
}
