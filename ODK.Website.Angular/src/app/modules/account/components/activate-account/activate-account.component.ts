import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { accountPaths } from '../../routing/account-paths';
import { AccountService } from 'src/app/services/account/account.service';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-activate-account',
  templateUrl: './activate-account.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivateAccountComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private chapterService: ChapterService,
    private accountService: AccountService
  ) {     
  }

  form: FormViewModel;
  links: {
    login: string;
  };
  submitted: boolean;

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    password: TextInputFormControlViewModel;
  };
  private token: string;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.token = this.route.snapshot.queryParamMap.get(accountPaths.activate.queryParams.token);
    this.links = {
      login: appUrls.login(this.chapter)
    };
    this.buildForm();    
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.accountService.activateAccount(this.formControls.password.value, this.token).subscribe((result: ServiceResult<void>) => {
      this.submitted = result.success;
      if (result.success) {
        this.form = null;
      }
      this.formCallback.next(result.messages);
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
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
      })
    };

    this.form = {
      buttons: [
        { text: 'Activate account' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.password
      ]
    };
  }
}
