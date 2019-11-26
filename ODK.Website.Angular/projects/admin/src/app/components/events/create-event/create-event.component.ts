import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from '../../../services/events/event.service';
import { NotificationService } from 'src/app/services/notifications/notification.service';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-create-event',
  templateUrl: './create-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateEventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private eventService: EventService,    
    private router: Router,
    private notificationService: NotificationService
  ) {     
  }  
  
  ngOnInit(): void {
    this.chapterService.getActiveChapter().subscribe((chapter: Chapter) => {
      this.chapter = chapter;
      this.changeDetector.detectChanges();
    });
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
