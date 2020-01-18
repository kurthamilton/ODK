import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { ServiceResult } from 'src/app/services/service-result';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-create-chapter-email-provider',
  templateUrl: './create-chapter-email-provider.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateChapterEmailProviderComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {     
  }

  breadcrumbs: MenuItem[];
  formCallback: Subject<string[]> = new Subject<string[]>();
  provider: ChapterEmailProvider;

  private chapter: Chapter;
  private providers: ChapterEmailProvider[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    
    this.breadcrumbs = [
      { link: adminUrls.emailProviders(this.chapter), text: 'Providers' }
    ];

    this.emailAdminService.getChapterAdminEmailProviders(this.chapter.id).subscribe((providers: ChapterEmailProvider[]) => {
      this.providers = providers;
      this.provider = this.createEmptyProvider();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(provider: ChapterEmailProvider): void {
    this.emailAdminService.addChapterEmailProvider(this.chapter.id, provider).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.emailProviders(this.chapter));
        return;
      }
    });
  }

  private createEmptyProvider(): ChapterEmailProvider {
    return {
      batchSize: 0,
      dailyLimit: 0,
      fromEmailAddress: this.providers.length > 0 ? this.providers[0].fromEmailAddress : '',
      fromName: this.providers.length > 0 ? this.providers[0].fromName : '',
      id: '',
      order: this.providers.length + 2,
      smtpLogin: '',
      smtpPassword: '',
      smtpPort: 587,
      smtpServer: ''
    };
  }
}
