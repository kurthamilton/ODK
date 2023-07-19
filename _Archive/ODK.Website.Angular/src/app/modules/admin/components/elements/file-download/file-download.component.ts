import { Component, OnChanges, ChangeDetectionStrategy, Input, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';

import { FileInfo } from 'src/app/core/files/file-info';
import { FileService } from 'src/app/services/files/file.service';
import { LoadingSpinnerOptions } from 'src/app/modules/shared/components/elements/loading-spinner/loading-spinner-options';

@Component({
  selector: 'app-file-download',
  templateUrl: './file-download.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FileDownloadComponent implements OnChanges {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private fileService: FileService
  ) {
  }

  @Input() file: FileInfo;
  @Input() linkText: string;
  @Input() tooltip: string;

  @ViewChild('downloadLink', { static: true }) downloadLink: ElementRef;

  downloading = false;
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true,
    small: true
  };

  ngOnChanges(): void {
    if (!this.linkText && this.file) {
      this.linkText = this.file.name.substr(0, this.file.name.length - `.${this.file.extension}`.length);
    }
  }

  onDownload(): void {
    this.downloading = true;
    this.changeDetector.detectChanges();

    this.fileService.downloadFile(this.file).subscribe((blob: Blob) => {
      this.performDownload(blob);

      this.downloading = false;
      this.changeDetector.detectChanges();
    });
  }

  private performDownload(blob: Blob): void {
    // IE and Edge
    if (window.navigator && window.navigator.msSaveOrOpenBlob) {
      window.navigator.msSaveOrOpenBlob(blob, this.file.name);
      return;
    }

    const url = window.URL.createObjectURL(blob);

    const link = this.downloadLink.nativeElement;
    link.href = url;
    link.download = this.file.name;
    link.click();

    window.URL.revokeObjectURL(url);
  }
}
