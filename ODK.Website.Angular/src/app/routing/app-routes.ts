import { Routes } from '@angular/router';

import { appPaths } from './app-paths';
import { AuthenticatedGuardService } from '../routing/authenticated-guard.service';
import { BlogComponent } from '../components/blogs/blog/blog.component';
import { ChapterAdminGuardService } from '../routing/chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { ChapterGuardService } from '../routing/chapter-guard.service';
import { ChapterLayoutComponent } from '../components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMemberGuardService } from '../routing/chapter-member-guard.service';
import { ContactComponent } from '../components/contact/contact/contact.component';
import { EventComponent } from '../components/events/event/event.component';
import { EventsComponent } from '../components/events/events/events.component';
import { FaqComponent } from '../components/about/faq/faq.component';
import { ForgottenPasswordComponent } from '../components/account/forgotten-password/forgotten-password.component';
import { HomeComponent } from '../components/home/home/home.component';
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
import { ChangePasswordComponent } from '../components/account/change-password/change-password.component';

const chapterPaths = appPaths.chapter.childPaths;

export const appRoutes: Routes = [
    {
      path: appPaths.home.path, component: HomeLayoutComponent, children: [
        { path: '', component: HomeComponent },
        { path: appPaths.join.path, component: JoinComponent },
        { path: appPaths.login.path, component: LoginComponent },
        { path: appPaths.logout.path, component: LogoutComponent },
        { path: appPaths.password.forgotten.path, component: ForgottenPasswordComponent },
        { path: appPaths.password.reset.path, component: ResetPasswordComponent },
        { path: appPaths.privacy.path, component: PrivacyComponent },
      ]
    },
    {
      path: appPaths.chapter.path, component: ChapterLayoutComponent, canActivate: [ChapterGuardService], children: [
        { path: '', component: ChapterComponent },
        { path: chapterPaths.blog.path, component: BlogComponent },
        { path: chapterPaths.contact.path, component: ContactComponent },
        { path: chapterPaths.events.path, component: EventsComponent },
        { path: chapterPaths.event.path, component: EventComponent },
        { path: chapterPaths.about.path, component: FaqComponent },
        { path: chapterPaths.members.path, component: MembersComponent, canActivate: [ChapterMemberGuardService] },
        { path: chapterPaths.member.path, component: MemberComponent, canActivate: [ChapterMemberGuardService] },
        { path: chapterPaths.profile.path, canActivate: [AuthenticatedGuardService], children: [
          { path: '', component: ProfileComponent },
          { path: chapterPaths.profile.password.change.path, component: ChangePasswordComponent },
          { path: chapterPaths.profile.subscription.path, component: SubscriptionComponent }
        ] },
      ]
    },
    { path: appPaths.admin.path, canLoad: [ChapterAdminGuardService], loadChildren: () => import('../admin/admin.module').then(m => m.AdminModule) }
  ];

  Object.freeze(appPaths);