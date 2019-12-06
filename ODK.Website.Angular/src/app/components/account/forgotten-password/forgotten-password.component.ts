import { Component, OnInit, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { Subject } from 'rxjs';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
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
  private controls: {
    email: TextInputFormControlViewModel
  } = {
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
  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      login: appUrls.login(this.chapter)
    };

    this.form = {
      buttonText: 'Submit',
      callback: this.formCallback,
      controls: [
        this.controls.email
      ]
    };
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.authenticationService.requestPasswordReset(this.controls.email.value).subscribe(() => {
      this.message = 'An email containing password reset instructions has been sent';
      this.changeDetector.detectChanges();
    });
  }
}
