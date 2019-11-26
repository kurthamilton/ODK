import { NgModule } from '@angular/core';
import { NgbModule, NgbAlertModule, NgbCollapseModule, NgbDatepickerModule, NgbDropdownModule, NgbModalModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
    imports: [
        NgbModule,
        NgbAlertModule,
        NgbCollapseModule,
        NgbDatepickerModule,
        NgbDropdownModule,
        NgbModalModule,
        NgbTooltipModule,
    ],
    exports: [
        NgbModule,
        NgbAlertModule,
        NgbCollapseModule,
        NgbDatepickerModule,
        NgbDropdownModule,
        NgbModalModule,
        NgbTooltipModule,
    ]    
  })
  export class AppStyleModule {}