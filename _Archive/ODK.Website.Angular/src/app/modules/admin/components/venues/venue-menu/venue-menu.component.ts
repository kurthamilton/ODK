import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-venue-menu',
  templateUrl: './venue-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenueMenuComponent implements OnInit {

  constructor(
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const venue: Venue = this.venueAdminService.getActiveVenue();

    this.menuItems = [
      { link: adminUrls.venue(chapter, venue), text: 'Edit', matchExactRoute: true },
      { link: adminUrls.venueEvents(chapter, venue), text: 'Events' }
    ];
  }
}
