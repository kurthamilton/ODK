import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { switchMap, tap } from 'rxjs/operators';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';

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
    private eventService: EventService
  ) { 
  }

  chapter: Chapter;
  event: Event;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.event.params.id);
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.eventService.getEvent(id, chapter.id))
    ).subscribe((event: Event) => {
      if (!event) {
        this.router.navigateByUrl(appUrls.events(this.chapter));
        return;
      }

      this.event = event;
      this.changeDetector.detectChanges();
    });
  }

}
