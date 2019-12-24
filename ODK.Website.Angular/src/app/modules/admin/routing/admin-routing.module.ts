import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminComponent } from '../components/admin/admin.component';
import { adminPaths } from './admin-paths';
import { ChapterAdminGuardService } from 'src/app/routing/chapter-admin-guard.service';
import { ChapterComponent } from '../components/chapters/chapter/chapter.component';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { CreateVenueComponent } from '../components/venues/create-venue/create-venue.component';
import { EventComponent } from '../components/events/event/event.component';
import { EventInvitesComponent } from '../components/events/event-invites/event-invites.component';
import { EventResponsesComponent } from '../components/events/event-responses/event-responses.component';
import { EventsComponent } from '../components/events/events/events.component';
import { MemberComponent } from '../components/members/member/member.component';
import { MembersComponent } from '../components/members/members/members.component';
import { VenueComponent } from '../components/venues/venue/venue.component';
import { VenuesComponent } from '../components/venues/venues/venues.component';

const routes: Routes = [
  { path: adminPaths.home.path, component: AdminComponent, canActivate: [ChapterAdminGuardService], children: [
    { path: '', component: ChapterComponent },
    { path: adminPaths.events.path, children: [
      { path: '', component: EventsComponent },
      { path: adminPaths.events.create.path, component: CreateEventComponent },
      { path: adminPaths.events.event.path, component: EventComponent },
      { path: `${adminPaths.events.event.path}/${adminPaths.events.event.invites.path}`, component: EventInvitesComponent },
      { path: `${adminPaths.events.event.path}/${adminPaths.events.event.responses.path}`, component: EventResponsesComponent },
    ] },
    { path: adminPaths.members.path, children: [
      { path: '', component: MembersComponent },
      { path: adminPaths.members.member.path, component: MemberComponent }
    ] },
    { path: adminPaths.venues.path, children: [
      { path: '', component: VenuesComponent },
      { path: adminPaths.venues.create.path, component: CreateVenueComponent },
      { path: adminPaths.venues.venue.path, component: VenueComponent }
    ] }
  ] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
