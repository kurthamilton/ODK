import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from '../../../services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { FormControlViewModel } from '../../forms/form-control.view-model';
import { FormViewModel } from '../../forms/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';

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
    password: FormControlViewModel;
    username: FormControlViewModel;
  };

  ngOnInit(): void {
    this.formControls = {
      password: {
        id: 'password',
        label: 'Password',
        validators: {
          required: true
        },
        type: 'password'
      },
      username: {
        id: 'username',
        label: 'Email',
        validators: {
          required: true
        },
      }
    };

    this.form = {
      buttonText: 'Sign In',
      callback: this.formCallback.asObservable(),
      formControls: [ this.formControls.username, this.formControls.password ]
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

