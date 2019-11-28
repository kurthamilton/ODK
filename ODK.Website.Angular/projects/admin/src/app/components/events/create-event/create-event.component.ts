import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from '../../../../../../../src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from '../../../../../../../src/app/services/events/event-admin.service';
import { NotificationService } from 'src/app/services/notifications/notification.service';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-create-event',
  templateUrl: './create-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateEventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterAdminService,
    private eventService: EventAdminService,    
    private router: Router,
    private notificationService: NotificationService
  ) {     
  }  
  
  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }  

  chapter: Chapter;
  formCallback: Subject<string[]> = new Subject<string[]>();

  onFormSubmit(event: Event): void {  
    this.eventService.createEvent(event).subscribe((result: ServiceResult<Event>) => {
      this.formCallback.next(result.messages);

      if (result.success) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
      }
    });
  }  
}
