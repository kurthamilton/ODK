import { DatePipe, CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AboutComponent } from './components/about/about/about.component';
import { ActivateAccountComponent } from './components/account/activate/activate-account.component';
import { AppComponent } from './components/app/app.component';
import { AppFormsModule } from './modules/forms/app-forms.module';
import { AppRoutingModule } from './routing/app-routing.module';
import { AppSharedModule } from './modules/shared/app-shared.module';
import { BodyComponent } from './components/structure/body/body.component';
import { ChangePasswordComponent } from './components/account/change-password/change-password.component';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterFooterComponent } from './components/structure/chapter-footer/chapter-footer.component';
import { ChapterHeaderComponent } from './components/structure/chapter-header/chapter-header.component';
import { ChapterLayoutComponent } from './components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMenuComponent } from './components/structure/chapter-menu/chapter-menu.component';
import { ChapterSidebarComponent } from './components/chapters/chapter-sidebar/chapter-sidebar.component';
import { ContactComponent } from './components/contact/contact/contact.component';
import { EventComponent } from './components/events/event/event.component';
import { EventListComponent } from './components/events/event-list/event-list.component';
import { EventsComponent } from './components/events/events/events.component';
import { EventSidebarAttendeesComponent } from './components/events/event-sidebar-attendees/event-sidebar-attendees.component';
import { EventSidebarComponent } from './components/events/event-sidebar/event-sidebar.component';
import { FooterComponent } from './components/structure/footer/footer.component';
import { ForgottenPasswordComponent } from './components/account/forgotten-password/forgotten-password.component';
import { HeaderComponent } from './components/structure/header/header.component';
import { HomeComponent } from './components/home/home/home.component';
import { HomeFooterComponent } from './components/structure/home-footer/home-footer.component';
import { HomeHeaderComponent } from './components/structure/home-header/home-header.component';
import { HomeLayoutComponent } from './components/layouts/home-layout/home-layout.component';
import { HomeMenuComponent } from './components/structure/home-menu/home-menu.component';
import { HttpAuthInterceptorService } from './services/http/http-auth-interceptor.service';
import { JoinComponent } from './components/account/join/join.component';
import { ListEventComponent } from './components/events/list-event/list-event.component';
import { LoginComponent } from './components/account/login/login.component';
import { LogoutComponent } from './components/account/logout/logout.component';
import { MemberComponent } from './components/members/member/member.component';
import { MemberProfileComponent } from './components/members/member-profile/member-profile.component';
import { MembersComponent } from './components/members/members/members.component';
import { ModalComponent } from './components/elements/modal/modal.component';
import { NotificationComponent } from './components/notifications/notification/notification.component';
import { NotificationsComponent } from './components/notifications/notifications/notifications.component';
import { PageTitleComponent } from './components/structure/page-title/page-title.component';
import { PageWithSidebarComponent } from './components/layouts/page-with-sidebar/page-with-sidebar.component';
import { PaymentButtonComponent } from './components/payments/payment-button/payment-button.component';
import { PrivacyComponent } from './components/privacy/privacy.component';
import { ProfileComponent } from './components/account/profile/profile.component';
import { ProfileEmailsComponent } from './components/account/profile-emails/profile-emails.component';
import { ProfileFormComponent } from './components/account/profile-form/profile-form.component';
import { ProfilePictureComponent } from './components/account/profile-picture/profile-picture.component';
import { PurchaseSubscriptionComponent } from './components/account/purchase-subscription/purchase-subscription.component';
import { ResetPasswordComponent } from './components/account/reset-password/reset-password.component';
import { SectionComponent } from './components/elements/section/section.component';
import { SocialMediaImageListComponent } from './components/social-media/social-media-image-list/social-media-image-list.component';
import { StripeFormComponent } from './components/payments/stripe-form/stripe-form.component';
import { SubscriptionAlertComponent } from './components/account/subscription-alert/subscription-alert.component';
import { SubscriptionComponent } from './components/account/subscription/subscription.component';
import { ThreeTenetsComponent } from './components/home/three-tenets/three-tenets.component';
import { UpdateEmailAddressComponent } from './components/account/update-email-address/update-email-address.component';
import { DeleteAccountComponent } from './components/account/delete-account/delete-account.component';
import { HeaderBrandComponent } from './components/structure/header-brand/header-brand.component';

@NgModule({
  declarations: [
    AboutComponent,
    ActivateAccountComponent,
    AppComponent,
    BodyComponent,
    ChangePasswordComponent,
    ChapterComponent,
    ChapterFooterComponent,
    ChapterHeaderComponent,
    ChapterLayoutComponent,
    ChapterMenuComponent,
    ChapterSidebarComponent,
    ContactComponent,
    EventComponent,
    EventListComponent,
    EventsComponent,
    EventSidebarAttendeesComponent,
    EventSidebarComponent,
    FooterComponent,
    ForgottenPasswordComponent,
    HeaderComponent,
    HomeComponent,
    HomeFooterComponent,
    HomeHeaderComponent,
    HomeLayoutComponent,
    HomeMenuComponent,
    JoinComponent,
    ListEventComponent,
    LoginComponent,
    LogoutComponent,
    MemberComponent,
    MemberProfileComponent,
    MembersComponent,
    ModalComponent,
    NotificationComponent,
    NotificationsComponent,
    PageTitleComponent,
    PageWithSidebarComponent,
    PaymentButtonComponent,
    PrivacyComponent,
    ProfileComponent,
    ProfileEmailsComponent,
    ProfileFormComponent,
    ProfilePictureComponent,
    PurchaseSubscriptionComponent,
    ResetPasswordComponent,
    SectionComponent,
    SocialMediaImageListComponent,
    StripeFormComponent,
    SubscriptionAlertComponent,
    SubscriptionComponent,
    ThreeTenetsComponent,
    UpdateEmailAddressComponent,
    DeleteAccountComponent,
    HeaderBrandComponent,
  ],
  imports: [
    AppFormsModule,
    AppRoutingModule,
    AppSharedModule,
    BrowserModule,
    CommonModule,
    HttpClientModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: HttpAuthInterceptorService, multi: true },
    { provide: DatePipe, useClass: DatePipe }
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
