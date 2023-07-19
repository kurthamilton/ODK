import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter } from '@angular/core';

import { MediaFile } from 'src/app/core/media/media-file';

@Component({
  selector: 'app-media-file',
  templateUrl: './media-file.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaFileComponent implements OnChanges {

  constructor() { }

  @Input() file: MediaFile;
  @Output() close: EventEmitter<void> = new EventEmitter<void>();

  ngOnChanges(): void {
  }

  onClose(): void {
    this.close.emit();
  }
}
