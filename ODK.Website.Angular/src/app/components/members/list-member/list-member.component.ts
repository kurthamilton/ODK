import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-list-member',
  templateUrl: './list-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListMemberComponent {
  
  @Input() link: string;
  @Input() maxWidth: number;
  @Input() member: Member;   
  @Input() size: 'sm';
}
