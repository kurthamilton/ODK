import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminComponent } from '../components/admin/admin.component';
import { adminPaths } from './admin-paths';
import { ChapterAdminGuardService } from 'src/app/routing/chapter-admin-guard.service';
import { ChapterAdminLayoutComponent } from '../components/chapters/chapter-admin-layout/chapter-admin-layout.component';
import { ChapterAdminMemberAddComponent } from '../components/chapters/chapter-admin-member-add/chapter-admin-member-add.component';
import { ChapterAdminMemberComponent } from '../components/chapters/chapter-admin-member/chapter-admin-member.component';
import { ChapterAdminMembersComponent } from '../components/chapters/chapter-admin-members/chapter-admin-members.component';
import { ChapterEmailProviderComponent } from '../components/emails/chapter-email-provider/chapter-email-provider.component';
import { ChapterEmailsComponent } from '../components/emails/chapter-emails/chapter-emails.component';
import { ChapterPaymentSettingsComponent } from '../components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterPropertiesComponent } from '../components/chapters/chapter-properties/chapter-properties.component';
import { ChapterPropertyComponent } from '../components/chapters/chapter-property/chapter-property.component';
import { ChapterQuestionsComponent } from '../components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from '../components/chapters/chapter-settings/chapter-settings.component';
import { ChapterSuperAdminGuardService } from 'src/app/routing/chapter-super-admin-guard.service';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { CreateVenueComponent } from '../components/venues/create-venue/create-venue.component';
import { DefaultEmailsComponent } from '../components/emails/default-emails/default-emails.component';
import { EditEventComponent } from '../components/events/edit-event/edit-event.component';
import { EditMemberImageComponent } from '../components/members/edit-member-image/edit-member-image.component';
import { EditVenueComponent } from '../components/venues/edit-venue/edit-venue.component';
import { EmailLayoutComponent } from '../components/emails/email-layout/email-layout.component';
import { EventInvitesComponent } from '../components/events/event-invites/event-invites.component';
import { EventLayoutComponent } from '../components/events/event-layout/event-layout.component';
import { EventsComponent } from '../components/events/events/events.component';
import { MemberLayoutComponent } from '../components/members/member-layout/member-layout.component';
import { MembersComponent } from '../components/members/members/members.component';
import { MemberSubscriptionComponent } from '../components/members/member-subscription/member-subscription.component';
import { VenueEventsComponent } from '../components/venues/venue-events/venue-events.component';
import { VenueLayoutComponent } from '../components/venues/venue-layout/venue-layout.component';
import { VenuesComponent } from '../components/venues/venues/venues.component';

const routes: Routes = [
  { path: '', component: AdminComponent, canActivate: [ChapterAdminGuardService], children: [
    { path: '', pathMatch: 'full', redirectTo: adminPaths.chapter.path },
    { path: adminPaths.chapter.path, component: ChapterAdminLayoutComponent, children: [
      { path: '', component: ChapterSettingsComponent },
      { path: adminPaths.chapter.about.path, component: ChapterQuestionsComponent },
      { path: adminPaths.chapter.adminMembers.path, children: [
        { path: '', component: ChapterAdminMembersComponent },
        { path: adminPaths.chapter.adminMembers.add.path, component: ChapterAdminMemberAddComponent },
        { path: adminPaths.chapter.adminMembers.adminMember.path, component: ChapterAdminMemberComponent }
      ] },
      { path: adminPaths.chapter.payments.path, component: ChapterPaymentSettingsComponent, 
        canActivate: [ChapterSuperAdminGuardService] },
      { path: adminPaths.chapter.properties.path, children: [
        { path: '', component: ChapterPropertiesComponent },
        { path: adminPaths.chapter.properties.property.path, component: ChapterPropertyComponent }
      ] }
    ] },
    { path: adminPaths.emails.path, component: EmailLayoutComponent, children: [
      { path: '', component: ChapterEmailsComponent },
      { path: adminPaths.emails.default.path, component: DefaultEmailsComponent, 
        canActivate: [ChapterSuperAdminGuardService] },
      { path: adminPaths.emails.emailProvider.path, component: ChapterEmailProviderComponent, 
        canActivate: [ChapterSuperAdminGuardService] }
    ] },
    { path: adminPaths.events.path, children: [
      { path: '', component: EventsComponent },
      { path: adminPaths.events.create.path, component: CreateEventComponent },
      { path: adminPaths.events.event.path, component: EventLayoutComponent, children: [
        { path: '', component: EditEventComponent },
        { path: adminPaths.events.event.invites.path, component: EventInvitesComponent }
      ] },
    ] },
    { path: adminPaths.members.path, children: [
      { path: '', component: MembersComponent },
      { path: adminPaths.members.member.path, component: MemberLayoutComponent, children: [
        { path: '', component: MemberSubscriptionComponent },
        { path: adminPaths.members.member.image.path, component: EditMemberImageComponent }
      ] }
    ] },
    { path: adminPaths.venues.path, children: [
      { path: '', component: VenuesComponent },
      { path: adminPaths.venues.create.path, component: CreateVenueComponent },
      { path: adminPaths.venues.venue.path, component: VenueLayoutComponent, children: [
        { path: '', component: EditVenueComponent },
        { path: adminPaths.venues.venue.events.path, component: VenueEventsComponent }
      ] }
    ] }
  ] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
