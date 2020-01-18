import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-chapter-email-provider',
  templateUrl: './chapter-email-provider.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailProviderComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;
  formCallback: Subject<boolean> = new Subject<boolean>();    
  provider: ChapterEmailProvider;

  private chapter: Chapter;  

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    
    const chapterEmailProviderId: string = this.route.snapshot.paramMap.get(adminPaths.emails.emailProviders.emailProvider.params.id);
    this.emailAdminService.getChapterAdminEmailProvider(this.chapter.id, chapterEmailProviderId).subscribe((provider: ChapterEmailProvider) => {
      if (!provider) {
        this.router.navigateByUrl(adminUrls.emailProviders(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.emailProviders(this.chapter), text: 'Providers' }
      ];

      this.provider = provider;      
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(provider: ChapterEmailProvider): void {
    this.emailAdminService.updateChapterAdminEmailProvider(this.chapter.id, provider).subscribe(() => {
      this.formCallback.next(true);
    });
  }  
}
