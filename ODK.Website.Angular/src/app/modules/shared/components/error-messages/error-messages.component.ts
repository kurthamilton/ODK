import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

@Component({
  selector: 'app-error-messages',
  templateUrl: './error-messages.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ErrorMessagesComponent {

  @Input() messages: string[];
  
}
