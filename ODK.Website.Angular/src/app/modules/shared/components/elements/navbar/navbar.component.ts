import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges } from '@angular/core';

import { MenuItem } from '../../../../../core/menus/menu-item';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  collapsed = true;

  @Input() breakpoint: string;
  @Input() fluid: boolean;
  @Input() menuItems: MenuItem[];
  @Input() menuItemGroups: MenuItem[][];

  groups: MenuItem[][];

  ngOnChanges(): void {
    if (!this.menuItems && !this.menuItemGroups) {
      this.groups = null;
      return;
    }

    if (this.menuItemGroups) {
      this.groups = this.menuItemGroups;
    } else {
      this.groups = [ this.menuItems ];
    }
  }

  collapse(): void {
    this.collapsed = true;
    this.changeDetector.detectChanges();
  }
}
