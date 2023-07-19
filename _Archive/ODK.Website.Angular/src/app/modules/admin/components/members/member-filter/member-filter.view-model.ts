import { SubscriptionType } from 'src/app/core/account/subscription-type';

export interface MemberFilterViewModel {
  name: string;
  types: SubscriptionType[];
}
