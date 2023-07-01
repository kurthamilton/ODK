import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { Email } from 'src/app/core/emails/email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { EmailType } from 'src/app/core/emails/email-type';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-chapter-email',
  templateUrl: './chapter-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  email: ChapterEmail;
  formCallback: Subject<boolean> = new Subject<boolean>();

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    const type: EmailType = parseInt(this.route.snapshot.paramMap.get(adminPaths.chapter.emails.email.params.type), 10);

    this.emailAdminService.getChapterEmail(this.chapter.id, type).subscribe((email: ChapterEmail) => {
      if (!email) {
        this.router.navigateByUrl(adminUrls.chapterEmails(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.chapterEmails(this.chapter), text: 'Emails' }
      ];

      this.email = email;
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(email: Email): void {
    this.email.subject = email.subject;
    this.email.htmlContent = email.htmlContent;

    this.emailAdminService.updateChapterEmail(this.chapter.id, this.email).pipe(
      switchMap(() => this.emailAdminService.getChapterEmail(this.chapter.id, this.email.type))
    ).subscribe((updated: ChapterEmail) => {
      this.email = updated;
      this.formCallback.next(true);
      this.changeDetector.detectChanges();
    });
  }
}
