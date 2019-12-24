import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';
import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';

@Component({
  selector: 'app-venue-layout',
  templateUrl: './venue-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenueLayoutComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {       
  }

  venue: Venue;

  private chapter: Chapter;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.venues.venue.params.id);
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.venueAdminService.getVenue(id).subscribe((venue: Venue) => {
      if (!venue) {
        this.router.navigateByUrl(adminUrls.venues(this.chapter));
        return;
      }

      this.venueAdminService.setActiveVenue(venue);
      this.venue = venue;
      this.changeDetector.detectChanges();
    });
  }
}
