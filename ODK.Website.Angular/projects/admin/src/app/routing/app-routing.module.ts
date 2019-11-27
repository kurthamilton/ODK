import { NgModule } from '@angular/core';
import { Routes, RouterModule, UrlSerializer, DefaultUrlSerializer, UrlTree } from '@angular/router';

import { adminPaths } from './admin-paths';
import { AuthenticatedGuardService } from './authenticated-guard.service';
import { ChapterAdminGuardService } from './chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { ChapterLayoutComponent } from '../components/layouts/chapter-layout/chapter-layout.component';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { EventComponent } from '../components/events/event/event.component';
import { EventsComponent } from '../components/events/events/events.component';
import { HomeComponent } from '../components/home/home/home.component';
import { LoginComponent } from '../components/account/login/login.component';

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

export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  parse(url: string): UrlTree { 
    return super.parse(url.toLowerCase());
  }
}

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
  providers: [
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer },
  ]
})
export class AppRoutingModule { }
