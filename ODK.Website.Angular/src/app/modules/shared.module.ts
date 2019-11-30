import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { AccountMenuComponent } from '../components/account/account-menu/account-menu.component';
import { ElementsModule } from './elements.module';
import { NavbarComponent } from '../components/structure/navbar/navbar.component';
import { MemberImageComponent } from '../components/members/member-image/member-image.component';

@NgModule({
  declarations: [
    AccountMenuComponent,
    MemberImageComponent,
    NavbarComponent
  ],
  imports: [
    CommonModule,
    ElementsModule,    
    RouterModule
  ],
  exports: [
    AccountMenuComponent,
    ElementsModule,    
    MemberImageComponent,
    NavbarComponent    
  ],
})
export class SharedModule { }
