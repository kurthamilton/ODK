import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { ServiceResult } from 'src/app/services/service-result';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-join',
  templateUrl: './join.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class JoinComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private accountService: AccountService,
    private titleService: TitleService
  ) {     
  }

  chapter: Chapter;
  chapterTexts: ChapterTexts;
  formCallback: Subject<string[]> = new Subject<string[]>();
  submitted = false;

  private image: File;
  private profile: AccountProfile;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.titleService.setRouteTitle(`Join the ${this.chapter.name} Drunken Knitwits`);

    this.chapterService.getChapterTexts(this.chapter.id).subscribe((texts: ChapterTexts) => {
      this.chapterTexts = texts;
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(profile: AccountProfile): void {    
    this.profile = profile;
    this.register();
  }

  onImageUpload(image: File): void {
    this.image = image;
    this.register();
  }

  private register(): void {
    if (!this.profile || !this.image) {      
      return;
    }

    this.accountService.register(this.chapter.id, this.profile, this.image).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.submitted = result.success === true;
      this.changeDetector.detectChanges();
    });
  }
}
