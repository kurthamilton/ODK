import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { accountPaths } from './account-paths';
import { ActivateAccountComponent } from '../components/activate-account/activate-account.component';
import { AuthenticatedGuardService } from 'src/app/routing/authenticated-guard.service';
import { ChangePasswordComponent } from '../components/change-password/change-password.component';
import { ChapterGuardService } from 'src/app/routing/chapter-guard.service';
import { DeleteAccountComponent } from '../components/delete-account/delete-account.component';
import { ForgottenPasswordComponent } from '../components/forgotten-password/forgotten-password.component';
import { JoinComponent } from '../components/join/join.component';
import { LogoutComponent } from '../components/logout/logout.component';
import { ProfileComponent } from '../components/profile/profile.component';
import { ProfileEmailsComponent } from '../components/profile-emails/profile-emails.component';
import { ResetPasswordComponent } from '../components/reset-password/reset-password.component';
import { SubscriptionComponent } from '../components/subscription/subscription.component';
import { UnauthenticatedGuardService } from 'src/app/routing/unauthenticated-guard.service';
import { UpdateEmailAddressComponent } from '../components/update-email-address/update-email-address.component';

const routes: Routes = [
  { path: '', canActivate: [AuthenticatedGuardService], children: [
    { path: '', component: ProfileComponent, data: { title: 'My profile' } },
    { path: accountPaths.delete.path, component: DeleteAccountComponent, data: { title: 'Delete my account' } },  
    { path: accountPaths.emails.path, component: ProfileEmailsComponent, data: { title: 'Email opt-in' } },
    { path: accountPaths.logout.path, component: LogoutComponent },
    { path: accountPaths.password.change.path, component: ChangePasswordComponent, data: { title: 'Change password' } },
    { path: accountPaths.subscription.path, component: SubscriptionComponent, data: { title: 'My subscription' } },
    { path: accountPaths.updateEmailAddress.path, component: UpdateEmailAddressComponent, data: { title: 'Update email address' } },
  ] },
  
  { path: '', canActivate: [UnauthenticatedGuardService], children: [
    { path: accountPaths.activate.path, component: ActivateAccountComponent, 
      data: { title: 'Activate account' } },
    { path: accountPaths.join.path, component: JoinComponent, canActivate: [ChapterGuardService] },
    { path: accountPaths.password.forgotten.path, component: ForgottenPasswordComponent, 
      data: { title: 'Forgotten password' } },
    { path: accountPaths.password.reset.path, component: ResetPasswordComponent, data: { title: 'Reset password' } }
  ] }          
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountRoutingModule { }
