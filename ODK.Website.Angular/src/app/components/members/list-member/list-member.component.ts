import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-list-member',
  templateUrl: './list-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListMemberComponent implements OnChanges {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {     
  }

  @Input() maxWidth: number;
  @Input() member: Member;
  
  image: string;
  links: {
    member: string;
  };

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }    

    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.links = {
      member: appUrls.member(chapter, this.member)
    };
    
    this.memberService.getMemberImage(this.member.id, this.maxWidth).subscribe((image: string) => {
      this.image = image;
      this.changeDetector.detectChanges();
    })
  }
}
