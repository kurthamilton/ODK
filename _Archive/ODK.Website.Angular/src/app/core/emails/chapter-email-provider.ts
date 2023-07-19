export interface ChapterEmailProvider {
  batchSize: number;
  dailyLimit: number;
  fromEmailAddress: string;
  fromName: string;
  id: string;
  order: number;
  smtpLogin: string;
  smtpPassword: string;
  smtpPort: number;
  smtpServer: string;
}
