import { DatePipe } from '@angular/common';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { forkJoin, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountService } from 'src/app/services/account/account.service';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { SubscriptionType } from 'src/app/core/account/subscription-type';

@Component({
  selector: 'app-subscription',
  templateUrl: './subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private accountService: AccountService,
    private chapterService: ChapterService,
    private datePipe: DatePipe
  ) {
  }

  breadcrumbs: MenuItem[];
  chapterSubscriptions: ChapterSubscription[];
  completedSubject: Subject<void> = new Subject<void>();
  form: FormViewModel;
  subscription: MemberSubscription;
  title: string;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

    this.breadcrumbs = [
      { link: appUrls.profile(this.chapter), text: 'Profile' }
    ];

    forkJoin([
      this.accountService.getSubscription().pipe(
        tap((subscription: MemberSubscription) => this.subscription = subscription)
      ),
      this.chapterService.getChapterSubscriptions(this.chapter.id).pipe(
        tap((chapterSubscriptions: ChapterSubscription[]) => this.chapterSubscriptions = chapterSubscriptions)
      )
    ]).subscribe(() => {
      this.title = this.subscription.type === SubscriptionType.Trial ? 'Purchase membership' : 'Renew membership';
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.completedSubject.complete();
  }

  onPurchase(chapterSubscription: ChapterSubscription, token: string): void {
    this.accountService.purchaseSubscription(chapterSubscription.id, token).subscribe((subscription: MemberSubscription) => {
      this.subscription = subscription;
      this.completedSubject.next();
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    const controls: FormControlViewModel[] = [
      new ReadOnlyFormControlViewModel({
        id: 'type',
        label: {
          text: 'Membership type'
        },
        value: SubscriptionType[this.subscription.type]
      })
    ];

    if (this.subscription.expiryDate) {
      controls.push(new ReadOnlyFormControlViewModel({
        id: 'end-date',
        label: {
          text: 'End date'
        },
        value: this.datePipe.transform(this.subscription.expiryDate, 'dd MMMM yyyy')
      }));
    }
    this.form = {
      buttonText: '',
      callback: null,
      controls
    };
  }
}
