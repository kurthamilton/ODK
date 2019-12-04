import { ChapterPaymentSettings } from './chapter-payment-settings';

export interface ChapterAdminPaymentSettings extends ChapterPaymentSettings {
    apiSecretKey: string;
}
