import { SubscriptionType } from '../account/subscription-type';

export interface ChapterSubscription {
    amount: number;
    chapterId: string;
    description: string;
    id: string;
    name: string;
    subscriptionType: SubscriptionType;
    title: string;
}