import { Component, OnInit, ChangeDetectionStrategy, Output, EventEmitter } from '@angular/core';

import { Subject } from 'rxjs';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { NotificationService } from 'src/app/services/notifications/notification.service';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangePasswordComponent implements OnInit {

  constructor(private authenticationService: AuthenticationService,
    private notificationService: NotificationService,
    private chapterService: ChapterService
  ) {
  }

  @Output() passwordUpdate: EventEmitter<boolean> = new EventEmitter<boolean>();

  breadcrumbs: MenuItem[];
  form: FormViewModel;

  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    confirmPassword: TextInputFormControlViewModel;
    currentPassword: TextInputFormControlViewModel;
    newPassword: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: appUrls.profile(chapter), text: 'Profile' }
    ];

    this.formControls = {
      confirmPassword: new TextInputFormControlViewModel({
        id: 'confirmPassword',
        inputType: 'password',
        label: {
          text: 'Confirm password'
        },
        validation: {
          required: true
        }
      }),
      currentPassword: new TextInputFormControlViewModel({
        id: 'password',
        inputType: 'password',
        label: {
          text: 'Current password'
        },
        validation: {
          required: true
        }
      }),
      newPassword: new TextInputFormControlViewModel({
        id: 'newPassword',
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
      buttonText: 'Update',
      callback: this.formCallback.asObservable(),
      controls: [
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
      .subscribe((result: ServiceResult<void>) => {
        this.formCallback.next(result.messages);

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
