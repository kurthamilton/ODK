import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnChanges, Input } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

import { MapService } from 'src/app/services/maps/map.service';

@Component({
  selector: 'app-google-map',
  templateUrl: './google-map.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoogleMapComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private mapService: MapService,
    private sanitizer: DomSanitizer
  ) {     
  }

  @Input() query: string;
  
  url: SafeUrl;

  ngOnChanges(): void {
    if (!this.query) {
      return;
    }

    this.mapService.getGoogleMapsUrl(this.query).subscribe((url: string) => {
      this.url = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      this.changeDetector.detectChanges();
    });    
  }
}
