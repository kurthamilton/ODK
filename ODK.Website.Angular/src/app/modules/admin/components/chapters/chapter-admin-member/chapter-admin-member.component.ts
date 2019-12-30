import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-admin-member',
  templateUrl: './chapter-admin-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterAdminMemberComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  adminMember: ChapterAdminMember;
  breadcrumbs: MenuItem[];
  form: FormViewModel;
  
  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    adminEmailAddress: TextInputFormControlViewModel;
    name: ReadOnlyFormControlViewModel;
    receiveContactEmails: CheckBoxFormControlViewModel;
    receiveNewMemberEmails: CheckBoxFormControlViewModel;
    sendEventEmails: CheckBoxFormControlViewModel;
    sendNewMemberEmails: CheckBoxFormControlViewModel;
  };
  private memberId: string;

  ngOnInit(): void {
    this.memberId = this.route.snapshot.paramMap.get(adminPaths.chapter.adminMembers.adminMember.params.memberId);
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.chapterAdminMembers(this.chapter), text: 'Admin members' }
    ]

    this.chapterAdminService.getChapterAdminMember(this.chapter.id, this.memberId).subscribe((adminMember: ChapterAdminMember) => {
      if (!adminMember) {
        this.router.navigateByUrl(adminUrls.chapterAdminMembers(this.chapter));
        return;
      }
      
      this.adminMember = adminMember;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.adminMember.adminEmailAddress = this.formControls.adminEmailAddress.value;
    this.adminMember.receiveContactEmails = this.formControls.receiveContactEmails.value;
    this.adminMember.receiveNewMemberEmails = this.formControls.receiveNewMemberEmails.value;
    this.adminMember.sendEventEmails = this.formControls.sendEventEmails.value;
    this.adminMember.sendNewMemberEmails = this.formControls.sendNewMemberEmails.value;

    this.chapterAdminService.updateChapterAdminMember(this.chapter.id, this.adminMember).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      adminEmailAddress: new TextInputFormControlViewModel({
        id: 'admin-email-address',
        label: {
          text: 'Admin email address'
        },
        validation: {
          pattern: FormControlValidationPatterns.email,
          required: true
        },
        value: this.adminMember.adminEmailAddress
      }),
      name: new ReadOnlyFormControlViewModel({
        id: 'name',
        label: {
          text: 'Name'
        },
        value: `${this.adminMember.firstName} ${this.adminMember.lastName}`
      }),
      receiveContactEmails: new CheckBoxFormControlViewModel({
        id: 'receive-contact-emails',
        label: {
          text: 'Receive contact emails'
        },
        value: this.adminMember.receiveContactEmails
      }),
      receiveNewMemberEmails: new CheckBoxFormControlViewModel({
        id: 'receive-new-member-emails',
        label: {
          text: 'Receive new member emails'
        },
        value: this.adminMember.receiveNewMemberEmails
      }),
      sendEventEmails: new CheckBoxFormControlViewModel({
        id: 'send-event-emails',
        label: {
          subtitle: 'Replies to an event email will go to this address',
          text: 'Send event emails'
        },
        value: this.adminMember.sendEventEmails
      }),
      sendNewMemberEmails: new CheckBoxFormControlViewModel({
        id: 'send-new-member-emails',
        label: {
          text: 'Send new member emails'
        },
        value: this.adminMember.sendNewMemberEmails
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.adminEmailAddress,
        this.formControls.receiveContactEmails,
        this.formControls.receiveNewMemberEmails,
        this.formControls.sendEventEmails,
        this.formControls.sendNewMemberEmails
      ]
    };
  }
}
