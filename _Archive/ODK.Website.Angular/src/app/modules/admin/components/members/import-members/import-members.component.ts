import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { FileInfo } from 'src/app/core/files/file-info';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-import-members',
  templateUrl: './import-members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportMembersComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService
  ) {
  }

  file: FileInfo;
  importing = false;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.file = {
      extension: 'csv',
      name: 'Member import.csv',
      url: this.memberAdminService.getMemberImportTemplateUrl(this.chapter.id)
    };
  }

  onUploadFile(files: FileList): void {
    if (files.length === 0) {
      return;
    }

    this.importing = true;
    this.changeDetector.detectChanges();

    this.memberAdminService.importMembers(this.chapter.id, files[0]).subscribe(() => {
      this.importing = false;
      this.changeDetector.detectChanges();
    });
  }
}
