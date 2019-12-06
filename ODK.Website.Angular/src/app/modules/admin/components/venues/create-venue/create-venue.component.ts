import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ServiceResult } from 'src/app/services/service-result';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-create-venue',
  templateUrl: './create-venue.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateVenueComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService,
    private router: Router
  ) {
  }

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
  }

  chapter: Chapter;
  formCallback: Subject<string[]> = new Subject<string[]>();

  onFormSubmit(venue: Venue): void {
    this.venueAdminService.createVenue(venue).subscribe((result: ServiceResult<Venue>) => {
      this.formCallback.next(result.messages);

      if (result.success) {
        this.router.navigateByUrl(adminUrls.venues(this.chapter));
      }
    });
  }
}
