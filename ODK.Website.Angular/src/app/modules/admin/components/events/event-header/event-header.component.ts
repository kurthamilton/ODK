import { Component, Input, ChangeDetectionStrategy, OnChanges, ChangeDetectorRef } from '@angular/core';

import { Event } from 'src/app/core/events/event';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-event-header',
  templateUrl: './event-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventHeaderComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private venueAdminService: VenueAdminService
  ) {     
  }

  @Input() event: Event;

  venue: Venue;

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }    

    this.venueAdminService.getVenue(this.event.venueId).subscribe((venue: Venue) => {
      this.venue = venue;
      this.changeDetector.detectChanges();
    });
  }

}
