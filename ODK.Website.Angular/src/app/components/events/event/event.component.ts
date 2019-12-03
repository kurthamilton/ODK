import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterService: ChapterService,
    private eventService: EventService,
    private titleService: TitleService
  ) {
  }

  breadcrumbs: MenuItem[];
  chapter: Chapter;
  event: Event;
  eventId: string;

  ngOnInit(): void {
    this.eventId = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.event.params.id);

    this.chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: appUrls.events(this.chapter), text: 'Events' }
    ];

    this.eventService.getEvent(this.eventId, this.chapter.id).subscribe((event: Event) => {
      if (!event) {
        this.router.navigateByUrl(appUrls.events(this.chapter));
        return;
      }

      this.titleService.setRouteTitle(event.name, 'Events');
      this.event = event;
      this.changeDetector.detectChanges();
    });
  }
}
