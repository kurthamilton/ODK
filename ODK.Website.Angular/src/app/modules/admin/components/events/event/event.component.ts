import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService    
  ) {
  }

  chapter: Chapter;
  event: Event;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.eventAdminService.getEvent(id).subscribe((event: Event) => {
      if (!event) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
        return;
      }

      this.event = event;
      this.changeDetector.detectChanges();
    });
  }
}
