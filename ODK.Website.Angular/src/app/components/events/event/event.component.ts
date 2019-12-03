import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

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

  breadcrumbs: MenuItem[];
  chapter: Chapter;
  event: Event;
  eventId: string;
  
  ngOnInit(): void {
    this.load().subscribe(() => {
      if (!this.event) {
        this.router.navigateByUrl(appUrls.events(this.chapter));
        return;
      }            

      this.changeDetector.detectChanges();
    });
  }  

  private load(): Observable<{}> {
    this.eventId = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.event.params.id);
    this.changeDetector.detectChanges();

    this.chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: appUrls.events(this.chapter), text: 'Events' }
    ];
    return this.eventService.getEvent(this.eventId, this.chapter.id).pipe(
      tap((event: Event) => this.event = event)
    );
  }  
}
