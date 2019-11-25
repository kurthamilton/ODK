import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter } from '@angular/core';

import { Notification } from 'src/app/core/notifications/notification';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationComponent {

  constructor() { }

  @Input() notification: Notification;
  @Output() dismiss: EventEmitter<boolean> = new EventEmitter<boolean>();

  onDismiss(): void {
    this.dismiss.emit(true);
  }
}
