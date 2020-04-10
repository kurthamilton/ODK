import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) {
  }

  form: FormViewModel;
  links: {
    login: string;
  };
  updated: boolean;

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    password: TextInputFormControlViewModel;
  };
  private token: string;

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get(appPaths.password.reset.queryParams.token);
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      login: appUrls.login(this.chapter)
    };
    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    const password = this.formControls.password.value;
    this.authenticationService.completePasswordReset(password, this.token).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.updated = true;
      this.form = null;
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.formControls = {
      password: new TextInputFormControlViewModel({
        id: 'password',
        inputType: 'password',
        label: {
          text: 'New password'
        },
        validation: {
          required: true
        }
      })
    };

    this.form = {
      buttons: [
        { text: 'Reset' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.password
      ]
    };
  }
}
