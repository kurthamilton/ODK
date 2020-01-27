import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

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

  files: MediaFile[];

  private chapter: Chapter;  

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.mediaAdminService.getMedia(this.chapter.id).subscribe((files: MediaFile[]) => {
      this.files = files;
      this.changeDetector.detectChanges();
    });
  }

}
