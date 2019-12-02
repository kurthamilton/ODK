import { DatePipe } from '@angular/common';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { AccountService } from 'src/app/services/account/account.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { FormControlViewModel } from '../../forms/form-control.view-model';
import { FormViewModel } from '../../forms/form.view-model';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { SubscriptionType } from 'src/app/core/account/subscription-type';
import { tap } from 'rxjs/operators';
import { forkJoin } from 'rxjs';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Chapter } from 'src/app/core/chapters/chapter';

@Component({
  selector: 'app-subscription',
  templateUrl: './subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private accountService: AccountService,
    private chapterService: ChapterService,
    private datePipe: DatePipe
  ) {     
  }

  chapterSubscriptions: ChapterSubscription[];
  form: FormViewModel;
  subscription: MemberSubscription;

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();

    forkJoin([
      this.accountService.getSubscription().pipe(
        tap((subscription: MemberSubscription) => this.subscription = subscription)
      ),
      this.chapterService.getChapterSubscriptions(chapter.id).pipe(
        tap((chapterSubscriptions: ChapterSubscription[]) => this.chapterSubscriptions = chapterSubscriptions)
      )
    ]).subscribe(() => {
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    const controls: FormControlViewModel[] = [
      {
        id: 'type',
        label: 'Membership type',
        type: 'readonly',
        value: SubscriptionType[this.subscription.type]
      }
    ];

    if (this.subscription.expiryDate) {
      controls.push({
        id: 'expirydate',
        label: 'End date',
        type: 'readonly',
        value: this.datePipe.transform(this.subscription.expiryDate, 'dd MMMM yyyy')
      });
    }
    this.form = {
      buttonText: '',
      callback: null,
      formControls: controls
    };
  }
}
