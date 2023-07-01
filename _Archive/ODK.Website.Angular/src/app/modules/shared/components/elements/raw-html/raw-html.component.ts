import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-raw-html',
  templateUrl: './raw-html.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RawHtmlComponent implements OnChanges {

  constructor(private sanitizer: DomSanitizer) { }

  @Input() html: string;

  parsed: SafeHtml;

  ngOnChanges(): void {
    this.parsed = this.sanitizer.bypassSecurityTrustHtml(this.html || '');
  }
}
