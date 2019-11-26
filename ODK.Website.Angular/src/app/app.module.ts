import { DatePipe } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AccountMenuComponent } from './components/account/account-menu/account-menu.component';
import { AppComponent } from './components/app/app.component';
import { AppRoutingModule } from './modules/app-routing.module';
import { BlogComponent } from './components/blogs/blog/blog.component';
import { BodyComponent } from './components/structure/body/body.component';
import { ChangePasswordComponent } from './components/account/change-password/change-password.component';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterFooterComponent } from './components/structure/chapter-footer/chapter-footer.component';
import { ChapterHeaderComponent } from './components/structure/chapter-header/chapter-header.component';
import { ChapterLayoutComponent } from './components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMenuComponent } from './components/structure/chapter-menu/chapter-menu.component';
import { ContactComponent } from './components/contact/contact/contact.component';
import { EventComponent } from './components/events/event/event.component';
import { EventListComponent } from './components/events/event-list/event-list.component';
import { EventsComponent } from './components/events/events/events.component';
import { FaqComponent } from './components/about/faq/faq.component';
import { FooterComponent } from './components/structure/footer/footer.component';
import { FormComponent } from './components/forms/form/form.component';
import { FormControlsComponent } from './components/forms/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/forms/form-control-validation/form-control-validation.component';
import { HeaderComponent } from './components/structure/header/header.component';
import { HomeComponent } from './components/home/home/home.component';
import { HomeFooterComponent } from './components/structure/home-footer/home-footer.component';
import { HomeHeaderComponent } from './components/structure/home-header/home-header.component';
import { HomeLayoutComponent } from './components/layouts/home-layout/home-layout.component';
import { HomeMenuComponent } from './components/structure/home-menu/home-menu.component';
import { HttpAuthInterceptorService } from './services/http/http-auth-interceptor.service';
import { ListEventComponent } from './components/events/list-event/list-event.component';
import { ListMemberComponent } from './components/members/list-member/list-member.component';
import { LoginComponent } from './components/account/login/login.component';
import { MemberComponent } from './components/members/member/member.component';
import { MemberListComponent } from './components/members/member-list/member-list.component';
import { MembersComponent } from './components/members/members/members.component';
import { ModalComponent } from './components/elements/modal/modal.component';
import { NavbarComponent } from './components/structure/navbar/navbar.component';
import { PageTitleComponent } from './components/structure/page-title/page-title.component';
import { PrivacyComponent } from './components/privacy/privacy.component';
import { ProfileComponent } from './components/account/profile/profile.component';
import { SectionComponent } from './components/elements/section/section.component';
import { SubscriptionComponent } from './components/account/subscription/subscription.component';
import { ThreeTenetsComponent } from './components/home/three-tenets/three-tenets.component';
import { NotificationsComponent } from './components/notifications/notifications/notifications.component';
import { NotificationComponent } from './components/notifications/notification/notification.component';
import { ProfilePictureComponent } from './components/account/profile-picture/profile-picture.component';
import { ProfileFormComponent } from './components/account/profile-form/profile-form.component';
import { FormControlComponent } from './components/forms/form-control/form-control.component';
import { AppStyleModule } from './modules/app-style.module';
import { ErrorMessagesComponent } from './components/elements/error-messages/error-messages.component';

@NgModule({
  declarations: [
    AccountMenuComponent,
    AppComponent,
    BlogComponent,
    BodyComponent,
    ChangePasswordComponent,
    ChapterComponent,
    ChapterFooterComponent,
    ChapterHeaderComponent,
    ChapterLayoutComponent,
    ChapterMenuComponent,
    ContactComponent,
    EventComponent,
    EventsComponent,
    EventListComponent,
    FaqComponent,
    FooterComponent,
    FormComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    HeaderComponent,
    HomeComponent,
    HomeFooterComponent,
    HomeHeaderComponent,
    HomeLayoutComponent,
    HomeMenuComponent,
    ListEventComponent,
    ListMemberComponent,
    LoginComponent,
    MemberComponent,
    MemberListComponent,
    MembersComponent,
    ModalComponent,
    NavbarComponent,
    PageTitleComponent,
    PrivacyComponent,
    ProfileComponent,
    SectionComponent,
    SubscriptionComponent,
    ThreeTenetsComponent,
    NotificationsComponent,
    NotificationComponent,
    ProfilePictureComponent,
    ProfileFormComponent,
    FormControlComponent,
    ErrorMessagesComponent,

  ],
  imports: [
    AppRoutingModule,
    AppStyleModule,
    BrowserModule,
    FormsModule,
    HttpClientModule,    
    ReactiveFormsModule
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
