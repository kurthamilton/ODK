import { Component, ChangeDetectionStrategy, Input, OnInit } from '@angular/core';

import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { Chapter } from 'src/app/core/chapters/chapter';
import { appUrls } from 'src/app/routing/app-urls';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberListComponent implements OnInit {

  constructor(private chapterService: ChapterService) {    
  }

  @Input() cols: number;
  @Input() hideName: boolean;
  @Input() members: Member[];
  
  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }

  getMemberLink(member: Member): string {
    return appUrls.member(this.chapter, member);
  }
}
