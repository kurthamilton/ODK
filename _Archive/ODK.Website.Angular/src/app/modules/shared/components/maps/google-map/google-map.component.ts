import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnChanges, Input } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

import { MapService } from 'src/app/services/maps/map.service';
import { Venue } from 'src/app/core/venues/venue';

@Component({
  selector: 'app-google-map',
  templateUrl: './google-map.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoogleMapComponent implements OnChanges {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private mapService: MapService,
    private sanitizer: DomSanitizer
  ) {
  }

  @Input() query: string;
  @Input() venue: Venue;

  url: SafeUrl;

  ngOnChanges(): void {
    if (!this.query && !this.venue) {
      return;
    }

    const venueId: string = this.venue ? this.venue.id : null;
    const query: string = this.query || (this.venue ? this.venue.mapQuery : null);

    this.mapService.getGoogleMapsUrl(venueId, query).subscribe((url: string) => {
      this.url = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      this.changeDetector.detectChanges();
    });
  }
}
