import { Component, OnInit, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { Subject } from 'rxjs';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-forgotten-password',
  templateUrl: './forgotten-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgottenPasswordComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) {
  }

  form: FormViewModel;
  links: {
    login: string;
  };
  message: string;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    email: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      login: appUrls.login(this.chapter)
    };

    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onAlertClose(): void {
    this.message = null;
    this.buildForm();
    this.changeDetector.detectChanges();
  }

  onFormSubmit(): void {
    this.authenticationService.requestPasswordReset(this.formControls.email.value).subscribe(() => {
      this.message = 'An email containing password reset instructions has been sent';
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.formControls = {
      email: new TextInputFormControlViewModel({
        id: 'email',
        label: {
          text: 'Email address'
        },
        value: '',
        validation: {
          required: true
        }
      })
    };

    this.form = {
      buttons: [
        { text: 'Submit' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.email
      ]
    };
  }
}
