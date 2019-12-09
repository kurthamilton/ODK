import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-join',
  templateUrl: './join.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class JoinComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private accountService: AccountService
  ) {     
  }

  chapter: Chapter;
  formCallback: Subject<string[]> = new Subject<string[]>();
  submitted = false;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(profile: AccountProfile): void {
    this.accountService.register(this.chapter.id, profile).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.submitted = result.success === true;
      this.changeDetector.detectChanges();
    });
  }
}
