import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-subscription-edit',
  templateUrl: './chapter-subscription-edit.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSubscriptionEditComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  breadcrumbs: MenuItem[];
  formCallback: Subject<string[]> = new Subject<string[]>();
  subscription: ChapterSubscription;

  private chapter: Chapter;  

  ngOnInit(): void {
    const subscriptionId: string = this.route.snapshot.paramMap.get(adminPaths.chapter.subscription.params.id);

    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.chapterSubscriptions(this.chapter), text: 'Subscriptions' }
    ];

    this.chapterAdminService.getChapterSubscription(subscriptionId).subscribe((subscription: ChapterSubscription) => {
      if (!subscription) {
        this.router.navigateByUrl(adminUrls.chapterSubscriptions(this.chapter));
        return;
      }
      this.subscription = subscription;
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(subscription: ChapterSubscription): void {    
    this.chapterAdminService.updateChapterSubscription(subscription).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.changeDetector.detectChanges();

      if (result.success) {
        window.scrollTo(0, 0);
      }
    });
  }
}
