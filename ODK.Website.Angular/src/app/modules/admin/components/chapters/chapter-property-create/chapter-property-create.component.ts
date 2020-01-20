import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { DataType } from 'src/app/core/data-types/data-type';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-property-create',
  templateUrl: './chapter-property-create.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPropertyCreateComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
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
    this.property = this.createEmptyProperty();    
    this.breadcrumbs = [
      { link: adminUrls.chapterProperties(this.chapter), text: 'Properties' }
    ];
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(property: ChapterProperty): void {
    this.chapterAdminService.createChapterProperty(this.chapter.id, property).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.chapterProperties(this.chapter));
      }      
    });
  }

  private createEmptyProperty(): ChapterProperty {
    return {
      dataType: DataType.None,
      helpText: '',
      hidden: false,
      id: '',
      label: '',
      name: '',
      required: false,
      subtitle: ''
    };
  }
}
