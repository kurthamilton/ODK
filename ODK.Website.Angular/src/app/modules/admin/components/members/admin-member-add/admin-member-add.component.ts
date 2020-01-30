import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { forkJoin, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-admin-member-add',
  templateUrl: './admin-member-add.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminMemberAddComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;
  members: Member[];

  private adminMembers: ChapterAdminMember[];
  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    member: DropDownFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.adminMembers(this.chapter), text: 'Admin members' }
    ];

    forkJoin([
      this.chapterAdminService.getChapterAdminMembers(this.chapter.id).pipe(
        tap((adminMembers: ChapterAdminMember[]) => this.adminMembers = adminMembers)
      ),
      this.memberAdminService.getMembers(this.chapter.id).pipe(
        tap((members: Member[]) => this.members = members)
      )
    ]).subscribe(() => {
      const adminMemberMap: Map<string, ChapterAdminMember> = ArrayUtils.toMap(this.adminMembers, x => x.memberId);

      this.members = this.members
        .filter(x => !adminMemberMap.has(x.id))
        .sort((a, b) => a.fullName.localeCompare(b.fullName));

      this.buildForm();

      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.chapterAdminService.addChapterAdminMember(this.chapter.id, this.formControls.member.value).subscribe(() => {
      this.formCallback.next(true);
      this.router.navigateByUrl(adminUrls.adminMembers(this.chapter));
    });
  }

  private buildForm(): void {
    this.formControls = {
      member: new DropDownFormControlViewModel({
        id: 'member',
        label: {
          text: 'Member'
        },
        options: [
          { text: '', value: '', default: true },
          ...this.members.map((x: Member): DropDownFormControlOption => ({
            text: x.fullName,
            value: x.id
          }))
        ]
      })
    };

    this.form = {
      buttons: [
        { text: 'Add' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.member
      ]
    };
  }
}
