import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { LazyLoadImageModule } from 'ng-lazyload-image';

import { AccountMenuComponent } from './components/account/account-menu/account-menu.component';
import { AppStyleModule } from '../style/app-style.module';
import { BodyComponent } from './components/structure/body/body.component';
import { BreadcrumbsComponent } from './components/elements/breadcrumbs/breadcrumbs.component';
import { ErrorMessagesComponent } from './components/error-messages/error-messages.component';
import { EventResponseIconComponent } from './components/events/event-response-icon/event-response-icon.component';
import { GoogleMapComponent } from './components/maps/google-map/google-map.component';
import { ListMemberComponent } from './components/members/list-member/list-member.component';
import { LoadingDirective } from './directives/loading/loading.directive';
import { LoadingSpinnerComponent } from './components/elements/loading-spinner/loading-spinner.component';
import { MemberImageComponent } from './components/members/member-image/member-image.component';
import { MemberListComponent } from './components/members/member-list/member-list.component';
import { ModalComponent } from './components/elements/modal/modal.component';
import { NavbarComponent } from './components/elements/navbar/navbar.component';
import { NotificationComponent } from './components/elements/breadcrumbs/notification/notification.component';
import { NotificationsComponent } from './components/elements/breadcrumbs/notifications/notifications.component';
import { PageTitleComponent } from './components/structure/page-title/page-title.component';
import { PaymentButtonComponent } from './components/payments/payment-button/payment-button.component';
import { PaypalFormComponent } from './components/payments/paypal-form/paypal-form.component';
import { RawHtmlComponent } from './components/elements/raw-html/raw-html.component';
import { StripeFormComponent } from './components/payments/stripe-form/stripe-form.component';
import { SubscriptionAlertComponent } from './components/account/subscription-alert/subscription-alert.component';

@NgModule({
  declarations: [
    AccountMenuComponent,
    BodyComponent,
    BreadcrumbsComponent,
    ErrorMessagesComponent,
    EventResponseIconComponent,
    GoogleMapComponent,
    ListMemberComponent,
    LoadingDirective,
    LoadingSpinnerComponent,
    MemberImageComponent,
    MemberListComponent,
    ModalComponent,
    NavbarComponent,
    NotificationComponent,
    NotificationsComponent,
    PageTitleComponent,
    PaymentButtonComponent,
    PaypalFormComponent,
    RawHtmlComponent,
    StripeFormComponent,
    SubscriptionAlertComponent
  ],
  imports: [
    AppStyleModule,
    CommonModule,
    LazyLoadImageModule,
    RouterModule,
  ],
  exports: [
    AccountMenuComponent,
    AppStyleModule,
    BodyComponent,
    BreadcrumbsComponent,
    ErrorMessagesComponent,
    EventResponseIconComponent,
    GoogleMapComponent,
    ListMemberComponent,
    LoadingDirective,
    LoadingSpinnerComponent,
    MemberImageComponent,
    MemberListComponent,
    ModalComponent,
    NavbarComponent,
    NotificationComponent,
    NotificationsComponent,
    PageTitleComponent,
    PaymentButtonComponent,
    PaypalFormComponent,
    RawHtmlComponent,
    StripeFormComponent,
    SubscriptionAlertComponent,
  ]
})
export class AppSharedModule { }
