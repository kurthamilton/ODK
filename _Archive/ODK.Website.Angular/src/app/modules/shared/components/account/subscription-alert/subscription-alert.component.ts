import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { forkJoin } from 'rxjs';
import { takeUntil, tap } from 'rxjs/operators';

import { AccountService } from 'src/app/services/account/account.service';
import { accountUrls } from 'src/app/modules/account/routing/account-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { DateUtils } from 'src/app/utils/date-utils';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { NotificationService } from 'src/app/services/notifications/notification.service';
import { SubscriptionType } from 'src/app/core/account/subscription-type';

const expiringSubscriptionAlertId = 'subscription-expiring';

@Component({
  selector: 'app-subscription-alert',
  templateUrl: './subscription-alert.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionAlertComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private notificationService: NotificationService,
    private accountService: AccountService
  ) {
  }

  action: string;
  disabled: boolean;
  expiresIn: string;
  hide = false;
  links: {
    subscription: string
  };
  status: string;
  type: string;

  private chapter: Chapter;
  private memberSubscription: MemberSubscription;

  ngOnInit(): void {
    this.authenticationService.authenticationTokenChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((token: AuthenticationToken) => {
      this.onTokenChange(token);
    });
  }

  ngOnDestroy(): void {}

  onClose(): void {
    this.hide = true;
    this.notificationService.dismissAlert(expiringSubscriptionAlertId);
    this.changeDetector.detectChanges();
  }

  private loadDetails(token: AuthenticationToken): void {
    this.status = '';

    const today: Date = DateUtils.today();
    const expires: Date = DateUtils.toDate(token.subscriptionExpiryDate);
    if (!token.subscriptionExpiryDate || expires > DateUtils.addDays(today, 7)) {
      this.changeDetector.detectChanges();
      return;
    }

    forkJoin([
      this.chapterService.getChapterById(token.chapterId).pipe(
        tap((chapter: Chapter) => this.chapter = chapter)
      ),
      this.accountService.getSubscription().pipe(
        tap((subscription: MemberSubscription) => this.memberSubscription = subscription)
      )
    ]).subscribe(() => {
      this.links = {
        subscription: accountUrls.subscription(this.chapter)
      };

      this.action = this.memberSubscription.type === SubscriptionType.Trial ? 'Purchase membership' : 'Renew';
      this.disabled = token.membershipDisabled;
      this.type = this.memberSubscription.type === SubscriptionType.Trial ? 'trial' : 'subscription';

      if (!token.subscriptionExpiryDate || expires < today) {
        this.status = 'expired';
        this.changeDetector.detectChanges();
        return;
      }

      if (this.notificationService.alertIsDismissed(expiringSubscriptionAlertId)) {
        this.changeDetector.detectChanges();
        return;
      }

      const expiresIn: number = DateUtils.daysBetween(today, expires);
      this.expiresIn = expiresIn === 1 ? 'tomorrow' : `in ${expiresIn} days`;
      this.status = 'expiring';
      this.changeDetector.detectChanges();
    });
  }

  private onTokenChange(token: AuthenticationToken): void {
    this.status = '';

    if (!token) {
      this.changeDetector.detectChanges();
      return;
    }

    this.loadDetails(token);
  }
}
