import { Component, OnInit, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';

import { Subject } from 'rxjs';

import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

@Component({
  selector: 'app-forgotten-password',
  templateUrl: './forgotten-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ForgottenPasswordComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService
  ) {
  }

  form: FormViewModel;
  links = {
    login: `/${appPaths.login.path}`
  }
  message: string;

  private controls: {
    email: TextInputViewModel
  } = {
    email: new TextInputViewModel({
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
