import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-breadcrumbs',
  templateUrl: './breadcrumbs.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BreadcrumbsComponent {
  @Input() active: string;
  @Input() breadcrumbs: MenuItem[];
}
