import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { ListEventViewModel } from './list-event.view-model';

@Component({
  selector: 'app-list-event',
  templateUrl: './list-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListEventComponent implements OnChanges {

  constructor(private chapterService: ChapterService) {
  }

  @Input() viewModel: ListEventViewModel;

  link: string;

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }

    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.link = appUrls.event(chapter, this.viewModel.event);
  }
}
