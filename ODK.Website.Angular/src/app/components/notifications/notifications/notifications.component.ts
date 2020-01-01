import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
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

  ngOnInit(): void {
    this.notificationService.subscribe()
      .pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe((notification: Notification) => {
        this.notifications.push(notification);
        this.changeDetector.detectChanges();
      });
  }

  ngOnDestroy(): void {}

  onDismiss(notification: Notification): void {
    const index: number = this.notifications.indexOf(notification);
    this.notifications.splice(index, 1);
    this.changeDetector.detectChanges();
  }
}
