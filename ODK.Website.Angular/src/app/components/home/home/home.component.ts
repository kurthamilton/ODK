import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {
  }

  chapters: Chapter[];
  countries: string[];

  ngOnInit(): void {
    this.chapterService.getChapters().subscribe((chapters: Chapter[]) => {
      this.chapters = chapters;
      this.countries = chapters
        .map(x => x.countryId)
        .filter((value: string, index: number, self: string[]) => self.indexOf(value) === index);
      this.changeDetector.detectChanges();
    });
  }
}
