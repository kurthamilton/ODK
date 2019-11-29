import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';

@Component({
  selector: 'app-chapter-sidebar',
  templateUrl: './chapter-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSidebarComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private authenticationService: AuthenticationService,
    private eventService: EventService
  ) {     
  }

  events: Event[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    
    const token: AuthenticationToken = this.authenticationService.getToken();
    if (token) {
      this.loadMemberPage();
    } else {
      this.loadPublicPage();
    }
  }

  getEventLink(event: Event): string {
    return appUrls.event(this.chapter, event);
  }

  private loadMemberPage(): void {
    this.eventService.getEvents(this.chapter.id).subscribe((events: Event[]) => {
      this.events = events;
      this.changeDetector.detectChanges();
    });
  }

  private loadPublicPage(): void {
    this.eventService.getPublicEvents(this.chapter.id).subscribe((events: Event[]) => {
      this.events = events;
      this.changeDetector.detectChanges();
    });
  }
}
