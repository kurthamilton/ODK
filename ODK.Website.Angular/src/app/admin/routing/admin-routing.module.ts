import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminComponent } from '../components/admin/admin.component';
import { adminPaths } from './admin-paths';
import { ChapterAdminGuardService } from 'src/app/routing/chapter-admin-guard.service';
import { EventsComponent } from '../components/events/events/events.component';
import { LoginComponent } from '../components/account/login/login.component';
import { EventComponent } from '../components/events/event/event.component';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { ChapterLayoutComponent } from '../components/layouts/chapter-layout/chapter-layout.component';
import { HomeComponent } from '../components/home/home/home.component';
import { AuthenticatedGuardService } from 'src/app/routing/authenticated-guard.service';

const routes: Routes = [
  { path: adminPaths.home.path, canActivate: [AuthenticatedGuardService], children: [
    { path: '', component: HomeComponent },
    { path: adminPaths.chapter.path, component: ChapterLayoutComponent, canActivate: [ChapterAdminGuardService], children: [
      { path: '', component: ChapterComponent },
      { path: adminPaths.events.path, children: [
        { path: '', component: EventsComponent },
        { path: adminPaths.events.create.path, component: CreateEventComponent },
        { path: adminPaths.events.event.path, component: EventComponent }
      ] }
    ] }
  ] },
  { path: adminPaths.login.path, component: LoginComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
