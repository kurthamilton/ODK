import { Routes } from '@angular/router';

import { AboutComponent } from '../components/about/about/about.component';
import { appPaths } from './app-paths';
import { ChapterAdminGuardService } from '../routing/chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { ChapterGuardService } from '../routing/chapter-guard.service';
import { ChapterLayoutComponent } from '../components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMemberGuardService } from '../routing/chapter-member-guard.service';
import { ContactComponent } from '../components/contact/contact/contact.component';
import { EventComponent } from '../components/events/event/event.component';
import { EventsComponent } from '../components/events/events/events.component';
import { HomeComponent } from '../components/home/home/home.component';
import { HomeGuardService } from './home-guard.service';
import { HomeLayoutComponent } from '../components/layouts/home-layout/home-layout.component';
import { LoginComponent } from '../components/account/login/login.component';
import { MemberComponent } from '../components/members/member/member.component';
import { MembersComponent } from '../components/members/members/members.component';
import { PrivacyComponent } from '../components/privacy/privacy.component';
import { UnauthenticatedGuardService } from './unauthenticated-guard.service';

const chapterPaths = appPaths.chapter.childPaths;

export const appRoutes: Routes = [
  {
    path: appPaths.home.path, component: HomeLayoutComponent, canActivate: [HomeGuardService], children: [
      { path: '', component: HomeComponent },
      
      { path: '', canActivate: [UnauthenticatedGuardService], children: [
        {
          path: chapterPaths.account.path,
          loadChildren: () => import('../modules/account/account.module').then(m => m.AccountModule)
        },        

        { path: appPaths.login.path, component: LoginComponent, data: { title: 'Login' } },
      ] },        
      { path: appPaths.privacy.path, component: PrivacyComponent, data: { title: 'Privacy' } },
    ]
  },
  
  {
    path: appPaths.chapter.path, component: ChapterLayoutComponent, canActivate: [ChapterGuardService], children: [
      { path: '', component: ChapterComponent },
      { path: chapterPaths.about.path, component: AboutComponent, data: { title: 'About' } },

      {
        path: chapterPaths.account.path,
        loadChildren: () => import('../modules/account/account.module').then(m => m.AccountModule)          
      },
      
      { path: 'profile/emails', redirectTo: 'account/emails' },

      { path: chapterPaths.contact.path, component: ContactComponent, data: { title: 'Send us a message' } },
      { path: chapterPaths.events.path, component: EventsComponent, data: { title: 'Events' } },
      { path: chapterPaths.event.path, component: EventComponent },        
      { path: chapterPaths.login.path, component: LoginComponent, canActivate: [UnauthenticatedGuardService], 
        data: { title: 'Login' } },        
      { path: chapterPaths.members.path, component: MembersComponent, canActivate: [ChapterMemberGuardService] },
      { path: chapterPaths.member.path, component: MemberComponent, canActivate: [ChapterMemberGuardService] }        
    ]
  },
  {
    path: appPaths.admin.path,
    canLoad: [ChapterAdminGuardService],
    loadChildren: () => import('../modules/admin/admin.module').then(m => m.AdminModule)
  },
  {
    path: '**', redirectTo: ''
  }
];

  Object.freeze(appPaths);