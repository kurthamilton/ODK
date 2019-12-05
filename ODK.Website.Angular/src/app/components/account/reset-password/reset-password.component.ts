import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';

import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ResetPasswordComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthenticationService
  ) {
  }

  form: FormViewModel;

  private controls: {
    password: FormControlViewModel;
  };
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private token: string;

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get(appPaths.password.reset.queryParams.token);

    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.authenticationService.completePasswordReset(this.controls.password.value, this.token).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.controls = {
      password: {
        id: 'password',
        label: {
          text: 'New password'
        },
        type: 'password',
        validators: {
          required: true
        }
      }
    };
    this.form = {
      buttonText: 'Reset',
      callback: this.formCallback,
      formControls: [
        this.controls.password
      ]
    };
  }
}
