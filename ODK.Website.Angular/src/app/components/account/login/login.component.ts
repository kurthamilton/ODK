import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from '../../../services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent implements OnInit {

  constructor(private authenticationService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router
  ) {
  }

  form: FormViewModel;
  links = {
    forgottenPassword: `/${appPaths.password.forgotten.path}`
  };

  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    password: TextInputViewModel;
    username: TextInputViewModel;
  };

  ngOnInit(): void {
    this.formControls = {
      password: new TextInputViewModel({
        id: 'password',
        inputType: 'password',
        label: {
          text: 'Password'
        },
        validation: {
          required: true
        }
      }),
      username: new TextInputViewModel({
        id: 'username',
        label: {
          text: 'Email'
        },
        validation: {
          required: true
        },
      })
    };

    this.form = {
      buttonText: 'Sign In',
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

