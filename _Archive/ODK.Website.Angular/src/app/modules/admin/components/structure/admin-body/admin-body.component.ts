import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-admin-body',
  templateUrl: './admin-body.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminBodyComponent {
  @Input() menuItems: MenuItem[];
}
