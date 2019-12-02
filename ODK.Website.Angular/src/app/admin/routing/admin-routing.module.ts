import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminComponent } from '../components/admin/admin.component';
import { adminPaths } from './admin-paths';
import { ChapterAdminGuardService } from 'src/app/routing/chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { EventsComponent } from '../components/events/events/events.component';
import { EventComponent } from '../components/events/event/event.component';
import { MemberComponent } from '../components/members/member/member.component';
import { MembersComponent } from '../components/members/members/members.component';

const routes: Routes = [
  { path: adminPaths.home.path, component: AdminComponent, canActivate: [ChapterAdminGuardService], children: [
    { path: '', component: ChapterComponent },
    { path: adminPaths.events.path, children: [
      { path: '', component: EventsComponent },
      { path: adminPaths.events.create.path, component: CreateEventComponent },
      { path: adminPaths.events.event.path, component: EventComponent },      
    ] },
    { path: adminPaths.members.path, children: [
      { path: '', component: MembersComponent },
      { path: adminPaths.members.member.path, component: MemberComponent }
    ] }
  ] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
