import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { NgbModule, NgbAlertModule, NgbCollapseModule, NgbDatepickerModule, NgbDropdownModule, NgbModalModule, NgbTabsetModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

import { AccountMenuComponent } from './components/account/account-menu/account-menu.component';
import { LoadingDirective } from './directives/loading/loading.directive';
import { LoadingSpinnerComponent } from './components/elements/loading-spinner/loading-spinner.component';
import { MemberImageComponent } from './components/members/member-image/member-image.component';
import { NavbarComponent } from './components/elements/navbar/navbar.component';

@NgModule({
  declarations: [
    AccountMenuComponent,
    MemberImageComponent,
    NavbarComponent,
    LoadingDirective,
    LoadingSpinnerComponent
  ],
  imports: [
    CommonModule,
    NgbModule,
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbTabsetModule,
    NgbTooltipModule,
    RouterModule
  ],
  exports: [
    AccountMenuComponent,
    LoadingDirective,
    LoadingSpinnerComponent,
    MemberImageComponent,
    NavbarComponent,
    NgbModule,
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbTabsetModule,
    NgbTooltipModule,
  ],
  entryComponents: [
    LoadingSpinnerComponent,
  ]
})
export class AppSharedModule { }