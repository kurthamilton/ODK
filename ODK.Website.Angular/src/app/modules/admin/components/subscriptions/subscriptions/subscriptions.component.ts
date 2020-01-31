import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';

@Component({
  selector: 'app-chapter-subscriptions',
  templateUrl: './subscriptions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private countryService: CountryService
  ) {
  }

  country: Country;
  paths: {
    create: string;
  };
  subscriptions: ChapterSubscription[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.paths = {
      create: adminUrls.subscriptionCreate(this.chapter)
    };

    forkJoin([
      this.chapterAdminService.getChapterSubscriptions(this.chapter.id).pipe(
        tap((subscriptions: ChapterSubscription[]) => this.subscriptions = subscriptions)
      ),
      this.countryService.getCountry(this.chapter.countryId).pipe(
        tap((country: Country) => this.country = country)
      )
    ]).subscribe(() => {
      this.subscriptions = this.subscriptions.sort((a, b) => b.amount - a.amount);
      this.changeDetector.detectChanges();
    });
  }

  getSubscriptionLink(subscription: ChapterSubscription): string {
    return adminUrls.subscription(this.chapter, subscription);
  }

  onDeleteSubscription(subscription: ChapterSubscription): void {
    if (!confirm('Are you sure you want to delete this subscription?')) {
      return;
    }

    this.subscriptions = null;
    this.changeDetector.detectChanges();

    this.chapterAdminService.deleteChapterSubscription(subscription).pipe(
      switchMap(() => this.chapterAdminService.getChapterSubscriptions(this.chapter.id))
    ).subscribe((subscriptions: ChapterSubscription[]) => {
      this.subscriptions = subscriptions.sort((a, b) => b.amount - a.amount);
      this.changeDetector.detectChanges();
    });
  }
}
