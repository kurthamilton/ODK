import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-subscription-create',
  templateUrl: './chapter-subscription-create.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSubscriptionCreateComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  breadcrumbs: MenuItem[];
  formCallback: Subject<string[]> = new Subject<string[]>();

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.chapterSubscriptions(this.chapter), text: 'Subscriptions' }
    ];
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(subscription: ChapterSubscription): void {
    this.chapterAdminService.createChapterSubscription(this.chapter.id, subscription).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.chapterSubscriptions(this.chapter));
        return;
      }
    });
  }
}
