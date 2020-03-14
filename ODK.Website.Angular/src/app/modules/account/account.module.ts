import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccountRoutingModule } from './routing/account-routing.module';
import { ActivateAccountComponent } from './components/activate-account/activate-account.component';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChangePasswordComponent } from './components/change-password/change-password.component';
import { DeleteAccountComponent } from './components/delete-account/delete-account.component';
import { ForgottenPasswordComponent } from './components/forgotten-password/forgotten-password.component';
import { JoinComponent } from './components/join/join.component';
import { LogoutComponent } from './components/logout/logout.component';
import { ProfileComponent } from './components/profile/profile.component';
import { ProfileEmailsComponent } from './components/profile-emails/profile-emails.component';
import { ProfileFormComponent } from './components/profile-form/profile-form.component';
import { ProfilePictureComponent } from './components/profile-picture/profile-picture.component';
import { PurchaseSubscriptionComponent } from './components/purchase-subscription/purchase-subscription.component';
import { ResetPasswordComponent } from './components/reset-password/reset-password.component';
import { SubscriptionComponent } from './components/subscription/subscription.component';
import { UpdateEmailAddressComponent } from './components/update-email-address/update-email-address.component';

@NgModule({
  declarations: [
    ActivateAccountComponent,
    ChangePasswordComponent,
    DeleteAccountComponent,
    ForgottenPasswordComponent,
    JoinComponent,
    LogoutComponent,
    ProfileComponent,
    ProfileEmailsComponent,
    ProfileFormComponent,
    ProfilePictureComponent,
    PurchaseSubscriptionComponent,
    ResetPasswordComponent,
    SubscriptionComponent,
    UpdateEmailAddressComponent,
  ],
  imports: [    
    AccountRoutingModule,
    AppFormsModule,
    AppSharedModule,
    CommonModule
  ]
})
export class AccountModule { }
