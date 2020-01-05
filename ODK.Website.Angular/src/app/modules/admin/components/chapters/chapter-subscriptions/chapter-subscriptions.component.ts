import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';

@Component({
  selector: 'app-chapter-subscriptions',
  templateUrl: './chapter-subscriptions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSubscriptionsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private countryService: CountryService
  ) {     
  }

  country: Country;
  subscriptions: ChapterSubscription[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.chapterAdminService.getChapterSubscriptions(this.chapter.id).pipe(
        tap((subscriptions: ChapterSubscription[]) => this.subscriptions = subscriptions)
      ),
      this.countryService.getCountry(this.chapter.countryId).pipe(
        tap((country: Country) => this.country = country)
      )
    ]).subscribe(() => {
      this.changeDetector.detectChanges();
    });
  }

  getSubscriptionLink(subscription: ChapterSubscription): string {
    return adminUrls.chapterSubscription(this.chapter, subscription);
  }
}
