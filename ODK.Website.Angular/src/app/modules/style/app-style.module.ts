import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { NgbModule, NgbAlertModule, NgbCollapseModule, NgbDatepickerModule, NgbDropdownModule, NgbModalModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    NgbModule,
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbTooltipModule,
  ],
  exports: [
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbModule,
    NgbTooltipModule,
  ]
})
export class AppStyleModule { }
