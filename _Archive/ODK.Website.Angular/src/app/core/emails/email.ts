import { EmailType } from './email-type';

export interface Email {
  htmlContent: string;
  name: string;
  subject: string;
  type: EmailType;
}
