import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef } from '@angular/core';

import { MenuItem } from './menu-item';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavbarComponent {
  
  constructor(private changeDetector: ChangeDetectorRef) {
  }

  collapsed = true;

  @Input() breakpoint: string;
  @Input() menuItems: MenuItem[];

  collapse(): void {
    this.collapsed = true;
    this.changeDetector.detectChanges();
  }
}
