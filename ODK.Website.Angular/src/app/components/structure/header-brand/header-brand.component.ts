import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';

@Component({
  selector: 'app-header-brand',
  templateUrl: './header-brand.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderBrandComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {   
  }

  imageUrl: string;
  link: string;

  ngOnInit(): void {
    this.chapterService.activeChapterChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((chapter: Chapter) => {
      if (chapter) {
        this.imageUrl = chapter.bannerImageUrl;
        this.link = appUrls.chapter(chapter);
      } else {
        this.imageUrl = '/assets/img/home-banner.png';
        this.link = '/';
      }

      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy() {}
}
