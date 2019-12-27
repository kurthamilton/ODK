import { ChapterEmailType } from './chapter-email-type';

export interface ChapterEmail {
  htmlContent: string;
  id: string;
  subject: string;
  type: ChapterEmailType;
}