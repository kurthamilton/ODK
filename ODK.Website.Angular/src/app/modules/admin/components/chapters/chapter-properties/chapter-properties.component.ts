import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

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

  links: {
    create: string;
  };
  properties: ChapterProperty[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.links = {
      create: adminUrls.chapterPropertyCreate(this.chapter)
    };

    this.chapterAdminService.getAdminChapterProperties(this.chapter.id).subscribe((properties: ChapterProperty[]) => {
      this.properties = properties;
      this.changeDetector.detectChanges();
    });
  }

  getPropertyLink(property: ChapterProperty): string {
    return adminUrls.chapterProperty(this.chapter, property);
  }

  onDeleteProperty(property: ChapterProperty): void {
    if (!confirm('Are you sure you want to delete this property?')) {
      return;
    }

    this.chapterAdminService.deleteChapterProperty(property).pipe(
      switchMap(() => this.chapterAdminService.getAdminChapterProperties(this.chapter.id))
    ).subscribe((properties: ChapterProperty[]) => {
      this.properties = properties;
      this.changeDetector.detectChanges();
    })
  }

  onMovePropertyDown(property: ChapterProperty): void {
    this.chapterAdminService.moveChapterPropertyDown(property.id).subscribe((properties: ChapterProperty[]) => {
      this.properties = properties;
      this.changeDetector.detectChanges();
    });
  }

  onMovePropertyUp(property: ChapterProperty): void {
    this.chapterAdminService.moveChapterPropertyUp(property.id).subscribe((properties: ChapterProperty[]) => {
      this.properties = properties;
      this.changeDetector.detectChanges();
    });
  }
}
