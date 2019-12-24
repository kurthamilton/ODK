import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';

@Component({
  selector: 'app-event-menu',
  templateUrl: './event-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventMenuComponent implements OnChanges {

  constructor(private chapterAdminService: ChapterAdminService) {
  }

  @Input() event: Event;

  links: {
    edit: string;
    invites: string;
    responses: string;
  };

  private chapter: Chapter;

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }

    this.chapter = this.chapterAdminService.getActiveChapter();
    this.links = {
      edit: adminUrls.event(this.chapter, this.event),
      invites: adminUrls.eventInvites(this.chapter, this.event),
      responses: adminUrls.eventResponses(this.chapter, this.event)
    };
  }

}
