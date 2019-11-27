import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { EventService } from '../../../services/events/event.service';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-edit-event',
  templateUrl: './edit-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditEventComponent {

  constructor(private router: Router,
    private eventService: EventService
  ) {     
  }

  @Input() chapter: Chapter;
  @Input() event: Event;

  formCallback: Subject<string[]> = new Subject<string[]>();

  onFormSubmit(event: Event): void {
    this.eventService.updateEvent(event).subscribe((result: ServiceResult<Event>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
      }
    });
  }
}
