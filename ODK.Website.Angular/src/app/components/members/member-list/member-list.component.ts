import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberListComponent {

  @Input() cols: number;
  @Input() members: Member[];
  @Input() size: string;
}
