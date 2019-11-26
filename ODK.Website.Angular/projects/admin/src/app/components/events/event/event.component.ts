import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from '../../../services/events/event.service';
import { ServiceResult } from 'src/app/services/service-result';
import { adminUrls } from '../../../routing/admin-urls';

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
  formCallback: Subject<string[]> = new Subject<string[]>();
    
  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);

    forkJoin([
      this.chapterService.getActiveChapter().pipe(
        tap((chapter: Chapter) => this.chapter = chapter)
      ),
      this.eventService.getEvent(id).pipe(
        tap((event: Event) => this.event = event)
      )
    ]).subscribe(() => {
      this.changeDetector.detectChanges();
    });
  }

  onFormSubmit(event: Event): void {
    this.eventService.updateEvent(event).subscribe((result: ServiceResult<Event>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
      }
    });
  }
}
