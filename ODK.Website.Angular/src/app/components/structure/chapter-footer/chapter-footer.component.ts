import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterService } from 'src/app/services/chapter/chapter.service';

@Component({
  selector: 'app-chapter-footer',
  templateUrl: './chapter-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterFooterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) { 
  }

  links: ChapterLinks;

  ngOnInit(): void {
    this.chapterService.getActiveChapter().pipe(
      switchMap((chapter: Chapter) => this.chapterService.getChapterLinks(chapter.id))
    ).subscribe((links: ChapterLinks) => {   
      this.links = links;      
      this.changeDetector.detectChanges();
    });
  }

}
