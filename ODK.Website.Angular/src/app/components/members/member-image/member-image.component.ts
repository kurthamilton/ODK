import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges } from '@angular/core';

import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-member-image',
  templateUrl: './member-image.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberImageComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private memberService: MemberService
  ) {     
  }

  @Input() maxWidth: number;
  @Input() member: Member;

  image: string;
  loading = true;

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }    

    this.loading = true;
    this.memberService.getMemberImage(this.member.id, this.maxWidth).subscribe((image: string) => {
      this.image = image;
      this.loading = false;
      this.changeDetector.detectChanges();
    })
  }
}
