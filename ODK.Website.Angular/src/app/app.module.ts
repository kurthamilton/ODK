import { CommonModule } from '@angular/common';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AboutComponent } from './components/about/about/about.component';
import { AppComponent } from './components/app/app.component';
import { AppFormsModule } from './modules/forms/app-forms.module';
import { AppRoutingModule } from './routing/app-routing.module';
import { AppSharedModule } from './modules/shared/app-shared.module';
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
import { HeaderComponent } from './components/structure/header/header.component';
import { HomeComponent } from './components/home/home/home.component';
import { HomeFooterComponent } from './components/structure/home-footer/home-footer.component';
import { HomeHeaderComponent } from './components/structure/home-header/home-header.component';
import { HomeLayoutComponent } from './components/layouts/home-layout/home-layout.component';
import { HomeMenuComponent } from './components/structure/home-menu/home-menu.component';
import { HttpAuthInterceptorService } from './services/http/http-auth-interceptor.service';
import { ListEventComponent } from './components/events/list-event/list-event.component';
import { LoginComponent } from './components/account/login/login.component';
import { MemberComponent } from './components/members/member/member.component';
import { MemberProfileComponent } from './components/members/member-profile/member-profile.component';
import { MembersComponent } from './components/members/members/members.component';
import { PageWithSidebarComponent } from './components/layouts/page-with-sidebar/page-with-sidebar.component';
import { PrivacyComponent } from './components/privacy/privacy.component';
import { SectionComponent } from './components/elements/section/section.component';
import { SocialMediaImageListComponent } from './components/social-media/social-media-image-list/social-media-image-list.component';
import { ThreeTenetsComponent } from './components/home/three-tenets/three-tenets.component';

@NgModule({
  declarations: [
    AboutComponent,
    AppComponent,    
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
    HeaderComponent,
    HomeComponent,
    HomeFooterComponent,
    HomeHeaderComponent,
    HomeLayoutComponent,
    HomeMenuComponent,
    ListEventComponent,
    LoginComponent,
    MemberComponent,
    MemberProfileComponent,
    MembersComponent,        
    PageWithSidebarComponent,
    PrivacyComponent,    
    SectionComponent,
    SocialMediaImageListComponent,        
    ThreeTenetsComponent,
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
    { provide: HTTP_INTERCEPTORS, useClass: HttpAuthInterceptorService, multi: true }
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
