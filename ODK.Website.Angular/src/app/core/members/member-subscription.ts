import { SubscriptionType } from '../account/subscription-type';

export interface MemberSubscription {
    expiryDate: Date;
    type: SubscriptionType;
}
