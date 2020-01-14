import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { Email } from 'src/app/core/emails/email';
import { EmailType } from 'src/app/core/emails/email-type';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';

const baseUrl: string = `${environment.apiBaseUrl}/admin/emails`;

const endpoints = {
  chapterEmail: (chapterId: string, type: EmailType) => `${baseUrl}/chapters/${chapterId}/${EmailType[type]}`,
  chapterEmails: (id: string) => `${baseUrl}/chapters/${id}`,
  chapterEmailProvider: (id: string, chapterEmailProviderId: string) => `${baseUrl}/chapters/${id}/providers/${chapterEmailProviderId}`,
  chapterEmailProviders: (id: string) => `${baseUrl}/chapters/${id}/providers`,
  email: (type: EmailType, chapterId: string) => `${baseUrl}/${EmailType[type]}?chapterId=${chapterId}`,
  emails: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  send: `${baseUrl}/send`
};

@Injectable({
  providedIn: 'root'
})
export class EmailAdminService {

  constructor(private http: HttpClient) { }

  addChapterEmailProvider(chapterId: string, provider: ChapterEmailProvider): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      batchSize: provider.batchSize ? provider.batchSize.toString() : null,
      dailyLimit: provider.dailyLimit.toString(),
      fromEmailAddress: provider.fromEmailAddress,
      fromName: provider.fromName,
      smtpLogin: provider.smtpLogin,
      smtpPassword: provider.smtpPassword,
      smtpPort: provider.smtpPort.toString(),
      smtpServer: provider.smtpServer
    });

    return this.http.post(endpoints.chapterEmailProviders(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  deleteChapterEmail(chapterId: string, type: EmailType): Observable<void> {
    return this.http.delete(endpoints.chapterEmail(chapterId, type)).pipe(
      map(() => undefined)
    );
  }

  deleteChapterEmailProvider(chapterId: string, chapterEmailProviderId: string): Observable<void> {
    return this.http.delete(endpoints.chapterEmailProvider(chapterId, chapterEmailProviderId)).pipe(
      map(() => undefined)
    );
  }

  getChapterAdminEmailProvider(chapterId: string, chapterEmailProviderId): Observable<ChapterEmailProvider> {
    return this.http.get(endpoints.chapterEmailProvider(chapterId, chapterEmailProviderId)).pipe(
      map((response: any) => this.mapChapterEmailProvider(response))
    );
  }

  getChapterAdminEmailProviders(chapterId: string): Observable<ChapterEmailProvider[]> {
    return this.http.get(endpoints.chapterEmailProviders(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterEmailProvider(x)))
    );
  }

  getChapterEmails(chapterId: string): Observable<ChapterEmail[]> {
    return this.http.get(endpoints.chapterEmails(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterEmail(x)))
    );
  }

  getEmails(currentChapterId: string): Observable<Email[]> {
    return this.http.get(endpoints.emails(currentChapterId)).pipe(
      map((response: any) => response.map(x => this.mapEmail(x)))
    );
  }

  sendEmail(memberId: string, subject: string, body: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      body: body,
      memberId: memberId,
      subject: subject
    });

    return this.http.post(endpoints.send, params).pipe(
      map(() => undefined)
    );
  }

  updateChapterAdminEmailProvider(chapterId: string, provider: ChapterEmailProvider): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      batchSize: provider.batchSize ? provider.batchSize.toString() : null,
      dailyLimit: provider.dailyLimit.toString(),
      fromEmailAddress: provider.fromEmailAddress,
      fromName: provider.fromName,
      smtpLogin: provider.smtpLogin,
      smtpPassword: provider.smtpPassword,
      smtpPort: provider.smtpPort.toString(),
      smtpServer: provider.smtpServer
    });

    return this.http.put(endpoints.chapterEmailProvider(chapterId, provider.id), params).pipe(
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

  private mapChapterEmailProvider(response: any): ChapterEmailProvider {
    return {
      batchSize: response.batchSize,
      dailyLimit: response.dailyLimit,
      fromEmailAddress: response.fromEmailAddress,
      fromName: response.fromName,
      id: response.id,
      order: response.order,
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
