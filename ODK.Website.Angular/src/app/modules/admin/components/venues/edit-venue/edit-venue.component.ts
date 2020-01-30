import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ServiceResult } from 'src/app/services/service-result';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-edit-venue',
  templateUrl: './edit-venue.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditVenueComponent implements OnInit, OnDestroy {

  constructor(private router: Router,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  chapter: Chapter;  
  formCallback: Subject<string[]> = new Subject<string[]>();
  venue: Venue;


  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.venue = this.venueAdminService.getActiveVenue();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(venue: Venue): void {
    this.venueAdminService.updateVenue(venue).subscribe((result: ServiceResult<Venue>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.venues(this.chapter));
      }
    });
  }
}
