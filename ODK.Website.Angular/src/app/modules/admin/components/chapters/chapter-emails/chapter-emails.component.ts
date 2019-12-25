import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailSettings } from 'src/app/core/chapters/chapter-email-settings';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-emails',
  templateUrl: './chapter-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  emailSettings: ChapterEmailSettings;
  form: FormViewModel;

  private chapter: Chapter;
  private emailProviders: string[];
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    adminEmailAddress: TextInputFormControlViewModel;
    contactEmailAddress: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.chapterAdminService.getChapterAdminEmailSettings(this.chapter.id).pipe(
        tap((emailSettings: ChapterEmailSettings) => this.emailSettings = emailSettings)
      ),
      this.chapterAdminService.getEmailProviders().pipe(
        tap((providers: string[]) => this.emailProviders = providers)
      )  
    ]).subscribe(() => {
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.emailSettings.adminEmailAddress = this.formControls.adminEmailAddress.value;
    this.emailSettings.contactEmailAddress = this.formControls.contactEmailAddress.value;
    this.chapterAdminService.updateChapterAdminEmailSettings(this.chapter.id, this.emailSettings).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      adminEmailAddress: new TextInputFormControlViewModel({
        id: 'admin-email-address',
        label: {
          text: 'Admin email address'          
        },
        validation: {
          required: true
        },
        value: this.emailSettings.adminEmailAddress
      }),
      contactEmailAddress: new TextInputFormControlViewModel({
        id: 'contact-email-address',
        label: {
          text: 'Contact email address'
        },
        validation: {
          required: true
        },
        value: this.emailSettings.contactEmailAddress
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [        
        this.formControls.adminEmailAddress,
        this.formControls.contactEmailAddress
      ]
    };
  }
}
