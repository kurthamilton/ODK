import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Email } from 'src/app/core/emails/email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { EmailType } from 'src/app/core/emails/email-type';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-default-email',
  templateUrl: './default-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DefaultEmailComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  email: Email;
  formCallback: Subject<boolean> = new Subject<boolean>();

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    const type: EmailType = parseInt(this.route.snapshot.paramMap.get(adminPaths.superAdmin.emails.email.params.type), 10);

    this.emailAdminService.getEmail(this.chapter.id, type).subscribe((email: Email) => {
      if (!email) {
        this.router.navigateByUrl(adminUrls.superAdminEmails(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.superAdminEmails(this.chapter), text: 'Emails' }
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

    this.emailAdminService.updateEmail(this.email, this.chapter.id).pipe(
      switchMap(() => this.emailAdminService.getEmail(this.chapter.id, this.email.type))
    ).subscribe((updated: Email) => {
      this.email = updated;
      this.formCallback.next(true);
      this.changeDetector.detectChanges();
    });
  }
}
