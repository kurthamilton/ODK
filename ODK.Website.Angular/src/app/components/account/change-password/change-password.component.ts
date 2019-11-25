import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { Subject } from 'rxjs';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { FormControlViewModel } from '../../forms/form-control.view-model';
import { FormViewModel } from '../../forms/form.view-model';
import { NotificationService } from 'src/app/services/notifications/notification.service';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangePasswordComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private notificationService: NotificationService
  ) { 
  }

  @Output() passwordUpdate: EventEmitter<boolean> = new EventEmitter<boolean>();

  form: FormViewModel;
  messages: string[];
  
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    confirmPassword: FormControlViewModel;
    currentPassword: FormControlViewModel;
    newPassword: FormControlViewModel;
  };
  
  ngOnInit(): void {
    this.formControls = {
      confirmPassword: {
        id: 'confirmPassword',
        label: 'Confirm password',
        validators: {
          required: true
        },
        type: 'password'
      },
      currentPassword: {
        id: 'password',
        label: 'Current password',
        validators: {
          required: true
        },
        type: 'password'
      },
      newPassword: {
        id: 'newPassword',
        label: 'New password',
        validators: {
          required: true
        },
        type: 'password'
      }
    };
    
    this.form = {
      buttonText: 'Update',
      callback: this.formCallback.asObservable(),
      formControls: [ 
        this.formControls.currentPassword, 
        this.formControls.newPassword,
        // this.formControls.confirmPassword
      ]
    };
  }

  onFormSubmit(): void {
    const currentPassword: string = this.formControls.currentPassword.value;
    const newPassword: string = this.formControls.newPassword.value;
    
    this.authenticationService
      .changePassword(currentPassword, newPassword)
      .subscribe((result: ServiceResult<{}>) => {
        this.formCallback.next(true);
        this.messages = result.messages;
        this.changeDetector.detectChanges();
        
        if (result.success === true) {
          this.notificationService.publish({
            message: 'Password updated',
            success: true
          });
          this.passwordUpdate.emit(true);
        }
      });
  }
}
