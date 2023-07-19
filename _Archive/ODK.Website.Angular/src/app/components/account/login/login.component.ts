import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { accountUrls } from 'src/app/modules/account/routing/account-urls';
import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from '../../../services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent implements OnInit {

  constructor(
    private authenticationService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router,
    private chapterService: ChapterService
  ) {
  }

  form: FormViewModel;
  links: {
    forgottenPassword: string
  };

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    password: TextInputFormControlViewModel;
    username: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      forgottenPassword: accountUrls.password.forgotten(this.chapter)
    };

    this.formControls = {
      password: new TextInputFormControlViewModel({
        id: 'password',
        inputType: 'password',
        label: {
          text: 'Password'
        },
        validation: {
          required: true
        }
      }),
      username: new TextInputFormControlViewModel({
        id: 'username',
        label: {
          text: 'Email address'
        },
        validation: {
          pattern: FormControlValidationPatterns.email,
          required: true
        },
      })
    };

    this.form = {
      buttons: [
        { text: 'Sign in' }
      ],
      callback: this.formCallback.asObservable(),
      controls: [ this.formControls.username, this.formControls.password ]
    };
  }

  onFormSubmit(): void {
    const password: string = this.formControls.password.value;
    const username: string = this.formControls.username.value;

    this.authenticationService
      .login(username, password)
      .subscribe((result: ServiceResult<AuthenticationToken>) => {
        this.formCallback.next(result.messages);

        if (result.success === true) {
          const url: string = this.route.snapshot.queryParams[appPaths.login.queryParams.returnUrl] || appPaths.home.path;
          this.router.navigateByUrl(url);
        }
      });
  }
}

