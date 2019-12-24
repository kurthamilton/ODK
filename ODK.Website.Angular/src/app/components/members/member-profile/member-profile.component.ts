import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MemberProfile } from 'src/app/core/members/member-profile';
import { MemberProperty } from 'src/app/core/members/member-property';
import { MemberService } from 'src/app/services/members/member.service';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';

@Component({
  selector: 'app-member-profile',
  templateUrl: './member-profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberProfileComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private datePipe: DatePipe,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {
  }

  @Input() memberId: string;

  form: FormViewModel;

  private chapterProperties: ChapterProperty[];
  private profile: MemberProfile;

  ngOnChanges(): void {
    if (!this.memberId) {
      return;
    }

    const chapter: Chapter = this.chapterService.getActiveChapter();

    forkJoin([
      this.memberService.getMemberProfile(this.memberId).pipe(
        tap((profile: MemberProfile) => this.profile = profile)
      ),
      this.chapterService.getChapterProperties(chapter.id).pipe(
        tap((properties: ChapterProperty[]) => this.chapterProperties = properties)
      )
    ]).subscribe(() => {
      this.buildProfileForm();
      this.changeDetector.detectChanges();
    });
  }

  private buildProfileForm(): void {
    const memberPropertyMap: Map<string, MemberProperty> = ArrayUtils.toMap(this.profile.properties, x => x.chapterPropertyId);
    this.form = {
      buttons: [],
      callback: null,
      controls: [
        ... this.chapterProperties
          .filter(x => memberPropertyMap.has(x.id) && !!memberPropertyMap.get(x.id).value)
          .map((x): FormControlViewModel => (new ReadOnlyFormControlViewModel({
            id: x.id,
            label: {
              text: x.name
            },
            value: memberPropertyMap.get(x.id).value
          }))),
        new ReadOnlyFormControlViewModel({
          id: 'joined',
          label: {
            text: 'Date joined'
          },
          value: this.datePipe.transform(this.profile.joined, 'dd MMMM yyyy')
        })
      ]
    };
  }
}
