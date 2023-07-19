import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';

@Component({
  selector: 'app-chapter-email-providers',
  templateUrl: './chapter-email-providers.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailProvidersComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  links: {
    createProvider: string;
  };
  providers: ChapterEmailProvider[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.links = {
      createProvider: adminUrls.emailProviderCreate(this.chapter)
    };

    this.emailAdminService.getChapterAdminEmailProviders(this.chapter.id).subscribe((providers: ChapterEmailProvider[]) => {
      this.providers = providers;
      this.changeDetector.detectChanges();
    });
  }

  onDeleteProvider(provider: ChapterEmailProvider): void {
    if (!confirm('Are you sure you want to delete this provider?')) {
      return;
    }

    this.emailAdminService.deleteChapterEmailProvider(this.chapter.id, provider.id).pipe(
      switchMap(() => this.emailAdminService.getChapterAdminEmailProviders(this.chapter.id))
    ).subscribe((providers: ChapterEmailProvider[]) => {
      this.providers = providers;
      this.changeDetector.detectChanges();
    });
  }

  getChapterEmailProviderLink(provider: ChapterEmailProvider): string {
    return adminUrls.emailProvider(this.chapter, provider);
  }
}
