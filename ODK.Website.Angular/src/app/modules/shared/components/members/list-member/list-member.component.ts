import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-list-member',
  templateUrl: './list-member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ListMemberComponent implements OnChanges {

  @Input() link: string;
  @Input() maxWidth: number;
  @Input() member: Member;
  @Input() size: 'sm' | 'xs';

  hideName: boolean;

  ngOnChanges(): void {
    switch (this.size) {
      case 'sm':
      case 'xs':
        this.hideName = true;
        break;
      default:
        this.hideName = false;
        break;
    }
  }
}
