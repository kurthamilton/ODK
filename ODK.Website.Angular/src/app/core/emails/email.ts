import { EmailType } from './email-type';

export interface Email {
  htmlContent: string;
  subject: string;
  type: EmailType;
}