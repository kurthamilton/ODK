import { Routes } from '@angular/router';

import { AboutComponent } from '../components/about/about/about.component';
import { appPaths } from './app-paths';
import { AuthenticatedGuardService } from '../routing/authenticated-guard.service';
import { ChangePasswordComponent } from '../components/account/change-password/change-password.component';
import { ChapterAdminGuardService } from '../routing/chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { ChapterGuardService } from '../routing/chapter-guard.service';
import { ChapterLayoutComponent } from '../components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMemberGuardService } from '../routing/chapter-member-guard.service';
import { ContactComponent } from '../components/contact/contact/contact.component';
import { EventComponent } from '../components/events/event/event.component';
import { EventsComponent } from '../components/events/events/events.component';
import { ForgottenPasswordComponent } from '../components/account/forgotten-password/forgotten-password.component';
import { HomeComponent } from '../components/home/home/home.component';
import { HomeGuardService } from './home-guard.service';
import { HomeLayoutComponent } from '../components/layouts/home-layout/home-layout.component';
import { JoinComponent } from '../components/account/join/join.component';
import { LoginComponent } from '../components/account/login/login.component';
import { LogoutComponent } from '../components/account/logout/logout.component';
import { MemberComponent } from '../components/members/member/member.component';
import { MembersComponent } from '../components/members/members/members.component';
import { PrivacyComponent } from '../components/privacy/privacy.component';
import { ProfileComponent } from '../components/account/profile/profile.component';
import { ResetPasswordComponent } from '../components/account/reset-password/reset-password.component';
import { SubscriptionComponent } from '../components/account/subscription/subscription.component';

const chapterPaths = appPaths.chapter.childPaths;

export const appRoutes: Routes = [
    {
      path: appPaths.home.path, component: HomeLayoutComponent, canActivate: [HomeGuardService], children: [
        { path: '', component: HomeComponent },
        { path: appPaths.login.path, component: LoginComponent, data: { title: 'Login' } },
        { path: appPaths.logout.path, component: LogoutComponent },
        { path: appPaths.password.forgotten.path, component: ForgottenPasswordComponent, data: { title: 'Forgotten password' } },
        { path: appPaths.password.reset.path, component: ResetPasswordComponent, data: { title: 'Reset password' } },
        { path: appPaths.privacy.path, component: PrivacyComponent, data: { title: 'Privacy' } },
      ]
    },
    {
      path: appPaths.chapter.path, component: ChapterLayoutComponent, canActivate: [ChapterGuardService], children: [
        { path: '', component: ChapterComponent },
        { path: chapterPaths.about.path, component: AboutComponent, data: { title: 'About' } },
        { path: chapterPaths.contact.path, component: ContactComponent, data: { title: 'Send us a message' } },
        { path: chapterPaths.events.path, component: EventsComponent, data: { title: 'Events' } },
        { path: chapterPaths.event.path, component: EventComponent },
        { path: chapterPaths.join.path, component: JoinComponent, data: { title: 'Join' } },
        { path: chapterPaths.login.path, component: LoginComponent, data: { title: 'Login' } },
        { path: chapterPaths.members.path, component: MembersComponent, canActivate: [ChapterMemberGuardService] },
        { path: chapterPaths.member.path, component: MemberComponent, canActivate: [ChapterMemberGuardService] },
        { path: chapterPaths.profile.path, canActivate: [AuthenticatedGuardService], children: [
          { path: '', component: ProfileComponent, data: { title: 'My profile' } },
          { path: chapterPaths.profile.password.change.path, component: ChangePasswordComponent, data: { title: 'Change password' } },
          { path: chapterPaths.profile.subscription.path, component: SubscriptionComponent, data: { title: 'My subscription' } }
        ] },
      ]
    },
    {
      path: appPaths.admin.path,
      canLoad: [ChapterAdminGuardService],
      loadChildren: () => import('../modules/admin/admin.module').then(m => m.AdminModule)
    }
  ];

  Object.freeze(appPaths);