import { Component, OnInit, ChangeDetectionStrategy, Input } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-event-sidebar-attendees',
  templateUrl: './event-sidebar-attendees.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventSidebarAttendeesComponent implements OnInit {

  constructor(private chapterService: ChapterService) {
  }

  @Input() members: Member[];
  @Input() title: string;

  chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }
}
