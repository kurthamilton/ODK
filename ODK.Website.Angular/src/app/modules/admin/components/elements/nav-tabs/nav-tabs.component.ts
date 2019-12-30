import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-nav-tabs',
  templateUrl: './nav-tabs.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavTabsComponent {

  @Input() menuItems: MenuItem[];

}
