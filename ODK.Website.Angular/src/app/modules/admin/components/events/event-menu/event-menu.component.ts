import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-event-menu',
  templateUrl: './event-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventMenuComponent implements OnChanges {

  constructor(private chapterAdminService: ChapterAdminService) {
  }

  @Input() event: Event;

  menuItems: MenuItem[];

  private chapter: Chapter;

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }

    this.chapter = this.chapterAdminService.getActiveChapter();
    this.menuItems = [
      { link: adminUrls.event(this.chapter, this.event), text: 'Edit', matchExactRoute: true },
      { link: adminUrls.eventInvites(this.chapter, this.event), text: 'Invites' },
      { link: adminUrls.eventResponses(this.chapter, this.event), text: 'Responses' },
    ];
  }

}
