import { NgModule } from '@angular/core';
import { NgbModule, NgbAlertModule, NgbCollapseModule, NgbDatepickerModule, NgbDropdownModule, NgbModalModule, NgbTooltipModule, NgbTabsetModule } from '@ng-bootstrap/ng-bootstrap';

@NgModule({
    imports: [
        NgbModule,
        NgbAlertModule,
        NgbCollapseModule,
        NgbDatepickerModule,
        NgbDropdownModule,
        NgbModalModule,
        NgbTabsetModule,
        NgbTooltipModule,
    ],
    exports: [
        NgbModule,
        NgbAlertModule,
        NgbCollapseModule,
        NgbDatepickerModule,
        NgbDropdownModule,
        NgbModalModule,
        NgbTabsetModule,
        NgbTooltipModule,
    ]    
  })
  export class AppStyleModule {}