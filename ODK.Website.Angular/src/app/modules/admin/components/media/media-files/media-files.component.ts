import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MediaAdminService } from 'src/app/services/media/media-admin.service';
import { MediaFile } from 'src/app/core/media/media-file';

@Component({
  selector: 'app-media-files',
  templateUrl: './media-files.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MediaFilesComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private mediaAdminService: MediaAdminService
  ) {     
  }

  copiedFile: MediaFile;
  files: MediaFile[];
  selectedFile: MediaFile;

  private chapter: Chapter;  

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.mediaAdminService.getMediaFiles(this.chapter.id).subscribe((files: MediaFile[]) => {
      this.files = files.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });
  }

  onAlertClose(): void {
    this.copiedFile = null;
    this.changeDetector.detectChanges();
  }

  onFileClick(file: MediaFile): void {
    this.selectedFile = file;
    this.changeDetector.detectChanges();
  }

  onFileClose(): void {
    this.selectedFile = null;
    this.changeDetector.detectChanges();
  }  

  onFileDelete(file: MediaFile): void {
    if (!confirm('Are you sure you want to delete this file?')) {
      return;
    }

    this.mediaAdminService.deleteMediaFile(this.chapter.id, file).subscribe((files: MediaFile[]) => {
      this.files = files.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });
  }  
  
  onFileUpload(files: FileList): void {
    if (files.length === 0) {
      return;
    }
 
    this.mediaAdminService.uploadMediaFile(this.chapter.id, files[0]).pipe(
      switchMap(() => this.mediaAdminService.getMediaFiles(this.chapter.id))
    ).subscribe((files: MediaFile[]) => {
      this.files = files.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });  
  }

  onFileUrlCopy(file: MediaFile): void {
    this.copiedFile = file;
    this.changeDetector.detectChanges();
  }
}
