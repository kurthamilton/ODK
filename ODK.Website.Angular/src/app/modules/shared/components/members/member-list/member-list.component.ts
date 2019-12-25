import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberListComponent implements OnChanges {

  @Input() chapter: Chapter;
  @Input() cols: number;
  @Input() members: Member[];
  @Input() size: 'sm' | 'xs';
  
  maxWidth: number;

  ngOnChanges(): void {
    switch (this.size) {
      case 'xs':
        this.maxWidth = 50;
        break;
      default:
        this.maxWidth = 150;
        break;
    }
  }

  getMemberLink(member: Member): string {
    return appUrls.member(this.chapter, member);
  }
}
