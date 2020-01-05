import { SubscriptionType } from '../account/subscription-type';

export interface ChapterSubscription {
    amount: number;
    chapterId: string;
    description: string;
    id: string;
    months: number;
    name: string;
    type: SubscriptionType;
    title: string;
}