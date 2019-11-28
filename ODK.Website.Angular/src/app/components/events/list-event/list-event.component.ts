import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';

@Component({
  selector: 'app-list-event',
  templateUrl: './list-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListEventComponent implements OnChanges {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {
  }

  @Input() event: Event;

  link: string;

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }

    this.chapterService.getChapterById(this.event.chapterId).subscribe((chapter: Chapter) => {
      this.link = appUrls.event(chapter, this.event);
      this.changeDetector.detectChanges();
    });
  }
}
