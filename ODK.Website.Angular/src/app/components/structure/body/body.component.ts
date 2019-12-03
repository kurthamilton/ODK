import { Component, ChangeDetectionStrategy, Input, OnInit } from '@angular/core';

import { MenuItem } from 'src/app/core/menus/menu-item';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-body',
  templateUrl: './body.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BodyComponent implements OnInit {

  constructor(private titleService: TitleService) {
  }

  @Input() breadcrumbs: MenuItem[];

  title: string;

  ngOnInit(): void {
    this.title = this.titleService.getRouteTitle();
  }
}
