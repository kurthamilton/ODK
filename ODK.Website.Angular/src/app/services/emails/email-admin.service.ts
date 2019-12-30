import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { ChapterEmailProviderSettings } from 'src/app/core/chapters/chapter-email-provider-settings';
import { Email } from 'src/app/core/emails/email';
import { EmailType } from 'src/app/core/emails/email-type';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';

const baseUrl: string = `${environment.apiBaseUrl}/admin/emails`;

const endpoints = {
  chapterEmail: (chapterId: string, type: EmailType) => `${baseUrl}/chapters/${chapterId}/${EmailType[type]}`,
  chapterEmails: (id: string) => `${baseUrl}/chapters/${id}`,
  chapterProviderSettings: (chapterId: string) => `${baseUrl}/chapters/${chapterId}/provider/settings`,
  email: (type: EmailType, chapterId: string) => `${baseUrl}/${EmailType[type]}?chapterId=${chapterId}`,
  emails: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  providers: `${baseUrl}/providers`
};

@Injectable({
  providedIn: 'root'
})
export class EmailAdminService {

  constructor(private http: HttpClient) { }

  deleteChapterEmail(chapterId: string, type: EmailType): Observable<void> {
    return this.http.delete(endpoints.chapterEmail(chapterId, type)).pipe(
      map(() => undefined)
    );
  }

  getChapterAdminEmailProviderSettings(chapterId: string): Observable<ChapterEmailProviderSettings> {
    return this.http.get(endpoints.chapterProviderSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterEmailProviderSettings(response))
    );
  }

  getChapterEmails(chapterId: string): Observable<ChapterEmail[]> {
    return this.http.get(endpoints.chapterEmails(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterEmail(x)))
    );
  }

  getEmailProviders(): Observable<string[]> {
    return this.http.get(endpoints.providers).pipe(
      map((response: any) => response)
    );
  }

  getEmails(currentChapterId: string): Observable<Email[]> {
    return this.http.get(endpoints.emails(currentChapterId)).pipe(
      map((response: any) => response.map(x => this.mapEmail(x)))
    );
  }

  updateChapterAdminEmailProviderSettings(chapterId: string, emailProviderSettings: ChapterEmailProviderSettings): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      apiKey: emailProviderSettings.apiKey,
      emailProvider: emailProviderSettings.emailProvider,
      fromEmailAddress: emailProviderSettings.fromEmailAddress,
      fromName: emailProviderSettings.fromName,
      smtpLogin: emailProviderSettings.smtpLogin,
      smtpPassword: emailProviderSettings.smtpPassword,
      smtpPort: emailProviderSettings.smtpPort.toString(),
      smtpServer: emailProviderSettings.smtpServer
    });

    return this.http.put(endpoints.chapterProviderSettings(chapterId), params).pipe(
      map(() => undefined)
    );
  }
  
  updateChapterEmail(chapterId: string, chapterEmail: ChapterEmail): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      htmlContent: chapterEmail.htmlContent,
      subject: chapterEmail.subject
    });

    return this.http.put(endpoints.chapterEmail(chapterId, chapterEmail.type), params).pipe(
      map(() => undefined)
    );
  }

  updateEmail(email: Email, currentChapterId: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      htmlContent: email.htmlContent,
      subject: email.subject
    });

    return this.http.put(endpoints.email(email.type, currentChapterId), params).pipe(
      map(() => undefined)
    );
  }

  private mapChapterEmail(response: any): ChapterEmail {
    return {
      htmlContent: response.htmlContent,
      id: response.id,
      subject: response.subject,
      type: response.type
    };
  }

  private mapChapterEmailProviderSettings(response: any): ChapterEmailProviderSettings {
    return {
      apiKey: response.apiKey,
      emailProvider: response.emailProvider,
      fromEmailAddress: response.fromEmailAddress,
      fromName: response.fromName,
      smtpLogin: response.smtpLogin,
      smtpPassword: response.smtpPassword,
      smtpPort: response.smtpPort,
      smtpServer: response.smtpServer
    };
  }

  private mapEmail(response: any): Email {
    return {
      htmlContent: response.htmlContent,
      subject: response.subject,
      type: response.type
    };
  }
}
