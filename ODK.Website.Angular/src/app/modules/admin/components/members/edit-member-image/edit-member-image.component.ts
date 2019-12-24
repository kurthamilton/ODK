import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-edit-member-image',
  templateUrl: './edit-member-image.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditMemberImageComponent implements OnInit, OnDestroy {

  constructor(private memberAdminService: MemberAdminService) {     
  }

  member: Member;
  updateImage: Subject<boolean> = new Subject<boolean>();
  
  ngOnInit(): void {
    this.member = this.memberAdminService.getActiveMember();
  }

  ngOnDestroy(): void {
    this.updateImage.complete();
  }

  onRotate(): void {
    this.memberAdminService.rotateMemberImage(this.member.id)
      .subscribe(() => this.updateImage.next(true));
  }
}
