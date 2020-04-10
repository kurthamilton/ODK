import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';

@Component({
  selector: 'app-chapter-emails',
  templateUrl: './chapter-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailsComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  emails: ChapterEmail[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.emailAdminService.getChapterEmails(this.chapter.id).subscribe((emails: ChapterEmail[]) => {
      this.emails = emails.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });
  }

  getEmailLink(email: ChapterEmail): string {
    return adminUrls.chapterEmail(this.chapter, email);
  }
}
