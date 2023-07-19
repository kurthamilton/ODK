import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-property',
  templateUrl: './chapter-property.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPropertyComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  formCallback: Subject<string[]> = new Subject<string[]>();
  property: ChapterProperty;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    const propertyId: string = this.route.snapshot.paramMap.get(adminPaths.chapter.properties.property.params.id);

    this.chapterAdminService.getChapterProperty(propertyId).subscribe((property: ChapterProperty) => {
      this.property = property;

      if (!this.property) {
        this.router.navigateByUrl(adminUrls.chapterProperties(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.chapterProperties(this.chapter), text: 'Properties' }
      ];

      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(property: ChapterProperty): void {
    this.chapterAdminService.updateChapterProperty(property).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
    });
  }
}
