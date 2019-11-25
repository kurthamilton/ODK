import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-list-member',
  templateUrl: './list-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListMemberComponent implements OnChanges {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {
  }

  @Input() member: Member;

  link: string;

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }

    this.chapterService.getChapterById(this.member.chapterId).subscribe((chapter: Chapter) => {
      this.link = appUrls.member(chapter, this.member);
      this.changeDetector.detectChanges();
    });
  }
}
