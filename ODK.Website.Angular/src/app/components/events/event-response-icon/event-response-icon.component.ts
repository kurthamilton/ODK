import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter } from '@angular/core';

import { EventResponseType } from 'src/app/core/events/event-response-type';

@Component({
  selector: 'app-event-response-icon',
  templateUrl: './event-response-icon.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventResponseIconComponent implements OnChanges {

  constructor() { }

  @Input() active: boolean;
  @Input() readOnly: boolean;
  @Input() type: EventResponseType;
  @Output() respond: EventEmitter<EventResponseType> = new EventEmitter<EventResponseType>();

  buttonClass: string;
  iconClass: string;
  tooltip: string;

  ngOnChanges(): void {
    this.buttonClass = '';
    this.iconClass = '';
    this.tooltip = '';

    if (this.type === EventResponseType.Yes) {
      this.buttonClass = 'yes';
      this.iconClass = 'fa-check-circle';
      this.tooltip = this.readOnly ? 'Going' : 'Yes';
    } else if (this.type === EventResponseType.Maybe) {
      this.buttonClass = 'maybe';
      this.iconClass = 'fa-question-circle';
      this.tooltip = this.readOnly ? 'Maybe going' : 'Maybe';
    } else if (this.type === EventResponseType.No) {
      this.buttonClass = 'no';
      this.iconClass = 'fa-times-circle';
      this.tooltip = this.readOnly ? 'Not going' : 'No';
    }
  }

  onClick(): void {
    this.respond.emit(this.type);
  }
}
