import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { ListVenueViewModel } from './list-venue.view-model';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';
import { VenueStats } from 'src/app/core/venues/venue-stats';

@Component({
  selector: 'app-venues',
  templateUrl: './venues.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenuesComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  links: {
    createVenue: string;
  };
  sortBy: string;
  viewModels: ListVenueViewModel[];

  private chapter: Chapter;
  private stats: VenueStats[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.links = {
      createVenue: adminUrls.venueCreate(this.chapter)
    };

    forkJoin([
      this.venueAdminService.getVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      ),
      this.venueAdminService.getChapterStats(this.chapter.id).pipe(
        tap((stats: VenueStats[]) => this.stats = stats)
      )
    ]).subscribe(() => {
      const statsMap: Map<string, VenueStats> = ArrayUtils.toMap(this.stats, x => x.venueId);

      this.viewModels = this.venues
        .sort((a, b) => a.name.localeCompare(b.name))
        .map((venue: Venue): ListVenueViewModel => ({
          stats: statsMap.get(venue.id),
          venue: venue
        }));
      this.changeDetector.detectChanges();
    });
  }

  getVenueLink(venue: Venue): string {
    return adminUrls.venue(this.chapter, venue);
  }

  onSort(sortBy: 'name' | 'stats.eventCount' | 'stats.lastEventDate'): void {
    switch (sortBy) {
      case 'stats.eventCount':
        this.viewModels = this.viewModels.sort((a, b) => b.stats.eventCount - a.stats.eventCount);
        break;
      case 'stats.lastEventDate':
        this.viewModels = this.viewModels.sort((a, b) => DateUtils.compare(b.stats.lastEventDate, a.stats.lastEventDate));
        break;
      default:
        this.viewModels = this.viewModels.sort((a, b) => a.venue.name.localeCompare(b.venue.name));
        break;
    }

    this.changeDetector.detectChanges();
  }
}
