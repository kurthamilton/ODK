import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-admin-sub-menu',
  templateUrl: './admin-sub-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminSubMenuComponent {

  @Input() menuItems: MenuItem[];
}
