import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
import { tap, switchMap } from 'rxjs/operators';
import { MemberService } from 'src/app/services/members/member.service';
import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {
  }

  chapter: Chapter;
  latestMembers: Member[];
  latestMemberImages: Map<string, string>;

  ngOnInit(): void {
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.memberService.getLatestMembers(chapter.id).pipe(
        tap((members: Member[]) => this.latestMembers = members)        
      ))
    ).subscribe(() => {
      this.memberService.getMemberImages(this.latestMembers.map(x => x.id)).subscribe((images: Map<string, string>) => {
        this.latestMemberImages = images;
        this.changeDetector.detectChanges();
      });
      this.changeDetector.detectChanges();
    });
  }
}
