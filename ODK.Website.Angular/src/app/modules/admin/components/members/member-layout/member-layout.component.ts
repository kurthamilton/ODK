import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { adminPaths } from '../../../routing/admin-paths';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-member-layout',
  templateUrl: './member-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberLayoutComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private memberAdminService: MemberAdminService
  ) {
  }

  member: Member;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.members.member.params.id);
    this.memberAdminService.getAdminMember(id).subscribe((member: Member) => {
      this.member = member;
      this.memberAdminService.setActiveMember(member);
      this.changeDetector.detectChanges();
    });
  }

}
