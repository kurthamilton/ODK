import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { Chapter } from 'src/app/core/chapters/chapter';
import { adminUrls } from '../../../routing/admin-urls';
import { MenuItem } from 'src/app/components/structure/navbar/menu-item';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterAdminService,
    private router: Router
  ) {
  }

  chapters: MenuItem[];

  ngOnInit(): void {
    this.chapterService.getChapters().subscribe((chapters: Chapter[]) => {
      if (chapters.length === 1) {
        this.router.navigateByUrl(adminUrls.chapter(chapters[0]));
      }

      this.chapters = chapters.map((chapter: Chapter): MenuItem => ({
        link: adminUrls.chapter(chapter),
        text: chapter.name
      }));
      this.changeDetector.detectChanges();
    });
  }

}
