import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';

@Component({
  selector: 'app-chapter-properties',
  templateUrl: './chapter-properties.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPropertiesComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) { 
  }

  properties: ChapterProperty[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterProperties(this.chapter.id).subscribe((properties: ChapterProperty[]) => {
      this.properties = properties;
      this.changeDetector.detectChanges();
    });
  }

  getPropertyLink(property: ChapterProperty): string {
    return adminUrls.chapterProperty(this.chapter, property);
  }
}
