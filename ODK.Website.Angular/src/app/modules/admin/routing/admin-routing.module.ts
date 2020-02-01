import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { AdminLayoutComponent } from '../components/structure/admin-layout/admin-layout.component';
import { AdminMemberAddComponent } from '../components/members/admin-member-add/admin-member-add.component';
import { AdminMemberComponent } from '../components/members/admin-member/admin-member.component';
import { AdminMembersComponent } from '../components/members/admin-members/admin-members.component';
import { adminPaths } from './admin-paths';
import { ChapterAdminGuardService } from 'src/app/routing/chapter-admin-guard.service';
import { ChapterAdminLayoutComponent } from '../components/chapters/chapter-admin-layout/chapter-admin-layout.component';
import { ChapterEmailComponent } from '../components/emails/chapter-email/chapter-email.component';
import { ChapterEmailProviderComponent } from '../components/emails/chapter-email-provider/chapter-email-provider.component';
import { ChapterEmailProvidersComponent } from '../components/emails/chapter-email-providers/chapter-email-providers.component';
import { ChapterEmailsComponent } from '../components/emails/chapter-emails/chapter-emails.component';
import { ChapterPaymentSettingsComponent } from '../components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterPropertiesComponent } from '../components/chapters/chapter-properties/chapter-properties.component';
import { ChapterPropertyComponent } from '../components/chapters/chapter-property/chapter-property.component';
import { ChapterPropertyCreateComponent } from '../components/chapters/chapter-property-create/chapter-property-create.component';
import { ChapterQuestionComponent } from '../components/chapters/chapter-question/chapter-question.component';
import { ChapterQuestionCreateComponent } from '../components/chapters/chapter-question-create/chapter-question-create.component';
import { ChapterQuestionsComponent } from '../components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from '../components/chapters/chapter-settings/chapter-settings.component';
import { ChapterSocialLinksComponent } from '../components/chapters/chapter-social-links/chapter-social-links.component';
import { ChapterSuperAdminGuardService } from 'src/app/routing/chapter-super-admin-guard.service';
import { CreateChapterEmailProviderComponent } from '../components/emails/create-chapter-email-provider/create-chapter-email-provider.component';
import { CreateEventComponent } from '../components/events/create-event/create-event.component';
import { CreateVenueComponent } from '../components/venues/create-venue/create-venue.component';
import { DefaultEmailComponent } from '../components/emails/default-email/default-email.component';
import { DefaultEmailsComponent } from '../components/emails/default-emails/default-emails.component';
import { EditEventComponent } from '../components/events/edit-event/edit-event.component';
import { EditMemberImageComponent } from '../components/members/edit-member-image/edit-member-image.component';
import { EditVenueComponent } from '../components/venues/edit-venue/edit-venue.component';
import { ErrorLogComponent } from '../components/super-admin/error-log/error-log.component';
import { EventAttendeesComponent } from '../components/events/event-attendees/event-attendees.component';
import { EventInvitesComponent } from '../components/events/event-invites/event-invites.component';
import { EventLayoutComponent } from '../components/events/event-layout/event-layout.component';
import { EventsAdminLayoutComponent } from '../components/events/events-admin-layout/events-admin-layout.component';
import { EventsComponent } from '../components/events/events/events.component';
import { MediaAdminLayoutComponent } from '../components/media/media-admin-layout/media-admin-layout.component';
import { MediaFilesComponent } from '../components/media/media-files/media-files.component';
import { MemberEventsComponent } from '../components/members/member-events/member-events.component';
import { MemberLayoutComponent } from '../components/members/member-layout/member-layout.component';
import { MembersAdminLayoutComponent } from '../components/members/members-admin-layout/members-admin-layout.component';
import { MembersComponent } from '../components/members/members/members.component';
import { MemberSubscriptionComponent } from '../components/members/member-subscription/member-subscription.component';
import { SendEmailComponent } from '../components/members/send-email/send-email.component';
import { SubscriptionCreateComponent } from '../components/subscriptions/subscription-create/subscription-create.component';
import { SubscriptionEditComponent } from '../components/subscriptions/subscription-edit/subscription-edit.component';
import { SubscriptionsComponent } from '../components/subscriptions/subscriptions/subscriptions.component';
import { SuperAdminLayoutComponent } from '../components/super-admin/super-admin-layout/super-admin-layout.component';
import { VenueEventsComponent } from '../components/venues/venue-events/venue-events.component';
import { VenueLayoutComponent } from '../components/venues/venue-layout/venue-layout.component';
import { VenuesComponent } from '../components/venues/venues/venues.component';

const routes: Routes = [
  { path: '', component: AdminLayoutComponent, canActivate: [ChapterAdminGuardService], children: [
    { path: '', pathMatch: 'full', redirectTo: adminPaths.chapter.path },    


    { path: adminPaths.chapter.path, component: ChapterAdminLayoutComponent, children: [
      { path: '', component: ChapterSettingsComponent },      

      { path: adminPaths.chapter.emails.path, children: [
        { path: '', component: ChapterEmailsComponent },
        { path: adminPaths.chapter.emails.email.path, component: ChapterEmailComponent }
      ] },

      { path: adminPaths.chapter.links.path, component: ChapterSocialLinksComponent },

      { path: adminPaths.chapter.properties.path, children: [
        { path: '', component: ChapterPropertiesComponent },
        { path: adminPaths.chapter.properties.create.path, component: ChapterPropertyCreateComponent },
        { path: adminPaths.chapter.properties.property.path, component: ChapterPropertyComponent }
      ] },

      { path: adminPaths.chapter.questions.path, children: [
        { path: '', component: ChapterQuestionsComponent },
        { path: adminPaths.chapter.questions.create.path, component: ChapterQuestionCreateComponent },
        { path: adminPaths.chapter.questions.question.path, component: ChapterQuestionComponent }
      ] }      
    ] },


    { path: adminPaths.events.path, component: EventsAdminLayoutComponent, children: [
      { path: adminPaths.venues.path, children: [
        { path: '', component: VenuesComponent },
        { path: adminPaths.venues.create.path, component: CreateVenueComponent },
        { path: adminPaths.venues.venue.path, component: VenueLayoutComponent, children: [
          { path: '', component: EditVenueComponent },
          { path: adminPaths.venues.venue.events.path, component: VenueEventsComponent }
        ] }
      ] },

      { path: '', component: EventsComponent },
      { path: adminPaths.events.create.path, component: CreateEventComponent },
      { path: adminPaths.events.event.path, component: EventLayoutComponent, children: [
        { path: '', component: EditEventComponent },
        { path: adminPaths.events.event.attendees.path, component: EventAttendeesComponent },
        { path: adminPaths.events.event.invites.path, component: EventInvitesComponent }
      ] }
    ] },


    { path: adminPaths.media.path, component: MediaAdminLayoutComponent, children: [
      { path: '', component: MediaFilesComponent }
    ] },


    { path: adminPaths.members.path, component: MembersAdminLayoutComponent, children: [
      { path: adminPaths.adminMembers.path, children: [
        { path: '', component: AdminMembersComponent },
        { path: adminPaths.adminMembers.add.path, component: AdminMemberAddComponent },
        { path: adminPaths.adminMembers.adminMember.path, component: AdminMemberComponent }
      ] },

      { path: adminPaths.subscriptions.path, children: [
        { path: '', component: SubscriptionsComponent },
        { path: adminPaths.subscriptions.create.path, component: SubscriptionCreateComponent },
        { path: adminPaths.subscriptions.subscription.path, component: SubscriptionEditComponent }
      ] },
      
      { path: '', component: MembersComponent },      
      { path: adminPaths.members.member.path, component: MemberLayoutComponent, children: [
        { path: '', component: MemberSubscriptionComponent },
        { path: adminPaths.members.member.events.path, component: MemberEventsComponent  },
        { path: adminPaths.members.member.image.path, component: EditMemberImageComponent },
        { path: adminPaths.members.member.sendEmail.path, component: SendEmailComponent }
      ] },
    ] },


    { path: adminPaths.superAdmin.path, component: SuperAdminLayoutComponent, canActivate: [ChapterSuperAdminGuardService],
    children: [
      { path: adminPaths.emailProviders.path, children: [
        { path:'', component: ChapterEmailProvidersComponent },
        { path: adminPaths.emailProviders.create.path, component: CreateChapterEmailProviderComponent },
        { path: adminPaths.emailProviders.emailProvider.path, component: ChapterEmailProviderComponent }
      ] },

      { path: adminPaths.superAdmin.emails.path, children: [
        { path: '', component: DefaultEmailsComponent },
        { path: adminPaths.superAdmin.emails.email.path, component: DefaultEmailComponent }
      ] },

      { path: adminPaths.superAdmin.errorLog.path, component: ErrorLogComponent },      
      { path: adminPaths.superAdmin.paymentSettings.path, component: ChapterPaymentSettingsComponent }
    ] },
  ] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
