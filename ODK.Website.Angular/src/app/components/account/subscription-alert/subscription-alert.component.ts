import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { DateUtils } from 'src/app/utils/date-utils';
import { NotificationService } from 'src/app/services/notifications/notification.service';

const expiringSubscriptionAlertId = 'subscription-expiring';

@Component({
  selector: 'app-subscription-alert',
  templateUrl: './subscription-alert.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionAlertComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private notificationService: NotificationService
  ) { 
  }
    
  expiresIn: string;
  hide = false;
  links: {
    subscription: string
  };
  
  status: string;  

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

  private onTokenChange(token: AuthenticationToken): void {
    this.status = '';

    if (!token) {      
      this.changeDetector.detectChanges();
      return;
    }

    this.chapterService.getChapterById(token.chapterId).subscribe((chapter: Chapter) => {
      this.links = {
        subscription: appUrls.subscription(chapter)
      };
      
      const today: Date = DateUtils.today();
      const expires: Date = DateUtils.toDate(token.subscriptionExpiryDate);    
  
      if (!token.subscriptionExpiryDate || expires < today) {
        this.status = 'expired';
        this.changeDetector.detectChanges();
        return;
      } 
      
      const isExpiring = expires < DateUtils.addDays(today, 7);
      if (!isExpiring) {
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
}
