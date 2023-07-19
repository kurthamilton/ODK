import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Email } from 'src/app/core/emails/email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';

@Component({
  selector: 'app-default-emails',
  templateUrl: './default-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DefaultEmailsComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  emails: Email[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.emailAdminService.getEmails(this.chapter.id).subscribe((emails: Email[]) => {
      this.emails = emails.sort((a, b) => a.name.localeCompare(b.name));
      this.changeDetector.detectChanges();
    });
  }

  getEmailLink(email: Email): string {
    return adminUrls.superAdminEmail(this.chapter, email);
  }
}
