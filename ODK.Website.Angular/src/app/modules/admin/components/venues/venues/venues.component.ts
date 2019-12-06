import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

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
  venues: Venue[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.links = {
      createVenue: adminUrls.venueCreate(this.chapter)
    };

    this.venueAdminService.getVenues(this.chapter.id).subscribe((venues: Venue[]) => {
      this.venues = venues.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });
  }

  getVenueLink(venue: Venue): string {
    return adminUrls.venue(this.chapter, venue);
  }
}
