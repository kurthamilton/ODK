import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { EventInvites } from 'src/app/core/events/event-invites';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
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
    private chapterService: ChapterAdminService,
    private eventService: EventAdminService
  ) {
  }

  chapter: Chapter;
  event: Event;
  invites: EventInvites;

  private responses: EventMemberResponse[];

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);
    this.chapter = this.chapterService.getActiveChapter();
    forkJoin([
      this.eventService.getEvent(id).pipe(
        tap((event: Event) => this.event = event)
      ),
      this.eventService.getEventInvites(id).pipe(
        tap((invites: EventInvites) => this.invites = invites)
      ),
      this.eventService.getEventResponses(id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      )
    ]).subscribe(() => {
      if (!this.event) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
        return;
      }

      this.changeDetector.detectChanges();
    });
  }

  onSendInvites(): void {
    this.eventService.sendInvites(this.event.id).subscribe(() => {

    });
  }
}
