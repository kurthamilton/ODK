import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { AdminListEventViewModel } from 'src/app/modules/admin/components/events/events/admin-list-event.view-model';

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

  @Input() viewModel: AdminListEventViewModel;

  link: string;

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }

    this.chapterService.getChapterById(this.viewModel.event.chapterId).subscribe((chapter: Chapter) => {
      this.link = appUrls.event(chapter, this.viewModel.event);
      this.changeDetector.detectChanges();
    });
  }
}
