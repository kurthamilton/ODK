import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { NgbModule, NgbAlertModule, NgbCollapseModule, NgbDatepickerModule, NgbDropdownModule, NgbModalModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

import { AccountMenuComponent } from './components/account/account-menu/account-menu.component';
import { GoogleMapComponent } from './components/maps/google-map/google-map.component';
import { ListMemberComponent } from './components/members/list-member/list-member.component';
import { LoadingDirective } from './directives/loading/loading.directive';
import { LoadingSpinnerComponent } from './components/elements/loading-spinner/loading-spinner.component';
import { MemberImageComponent } from './components/members/member-image/member-image.component';
import { MemberListComponent } from './components/members/member-list/member-list.component';
import { NavbarComponent } from './components/elements/navbar/navbar.component';
import { RawHtmlComponent } from './components/elements/raw-html/raw-html.component';

@NgModule({
  declarations: [
    AccountMenuComponent,
    GoogleMapComponent,
    ListMemberComponent,
    MemberImageComponent,
    MemberListComponent,
    NavbarComponent,
    LoadingDirective,
    LoadingSpinnerComponent,
    RawHtmlComponent,
  ],
  imports: [
    CommonModule,
    NgbModule,
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbTooltipModule,    
    RouterModule
  ],
  exports: [
    AccountMenuComponent,
    GoogleMapComponent,
    ListMemberComponent,
    LoadingDirective,
    LoadingSpinnerComponent,
    MemberImageComponent,
    MemberListComponent,
    NavbarComponent,
    NgbModule,
    NgbAlertModule,
    NgbCollapseModule,
    NgbDatepickerModule,
    NgbDropdownModule,
    NgbModalModule,
    NgbTooltipModule,
    RawHtmlComponent
  ],
  entryComponents: [
    LoadingSpinnerComponent,
  ]
})
export class AppSharedModule { }
