import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterEmailSettings } from 'src/app/core/chapters/chapter-email-settings';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from './chapter.service';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { ChapterEmailProviderSettings } from 'src/app/core/chapters/chapter-email-provider-settings';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  chapters: baseUrl,  
  emailProviders: `${baseUrl}/emails/providers`,
  emailProviderSettings: (id: string) => `${baseUrl}/${id}/emails/provider/settings`,
  emailSettings: (id: string) => `${baseUrl}/${id}/emails/settings`,
  paymentSettings: (id: string) => `${baseUrl}/${id}/payments/settings`,
  questions: (id: string) => `${baseUrl}/${id}/questions`,
  texts: (id: string) => `${baseUrl}/${id}/texts`,
};

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminService extends ChapterService {

  constructor(http: HttpClient) {
    super(http);
  }

  createChapterQuestion(chapterId: string, chapterQuestion: ChapterQuestion): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      answer: chapterQuestion.answer,
      name: chapterQuestion.name
    });

    return this.http.post(endpoints.questions(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  getAdminChapters(): Observable<Chapter[]> {
    return this.http.get(endpoints.chapters).pipe(
      map((response: any) => response.map(x => this.mapChapter(x)))
    );
  }

  getChapterAdminEmailProviderSettings(chapterId: string): Observable<ChapterEmailProviderSettings> {
    return this.http.get(endpoints.emailProviderSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterEmailProviderSettings(response))
    );
  }

  getChapterAdminEmailSettings(chapterId: string): Observable<ChapterEmailSettings> {
    return this.http.get(endpoints.emailSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterEmailSettings(response))
    );
  }

  getChapterAdminPaymentSettings(chapterId: string): Observable<ChapterAdminPaymentSettings> {
    return this.http.get(endpoints.paymentSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterAdminPaymentSettings(response))
    );
  }

  getEmailProviders(): Observable<string[]> {
    return this.http.get(endpoints.emailProviders).pipe(
      map((response: any) => response)
    );
  }

  hasAccess(chapter: Chapter): Observable<boolean> {
    return chapter ? this.getAdminChapters().pipe(
      map((chapters: Chapter[]) => !!chapters.find(x => x.id === chapter.id))
    ) : of(false);
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

    return this.http.put(endpoints.emailProviderSettings(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  updateChapterAdminEmailSettings(chapterId: string, emailSettings: ChapterEmailSettings): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      adminEmailAddress: emailSettings.adminEmailAddress,
      contactEmailAddress: emailSettings.contactEmailAddress
    });

    return this.http.put(endpoints.emailSettings(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  updateChapterAdminPaymentSettings(chapterId: string, paymentSettings: ChapterAdminPaymentSettings): Observable<ChapterAdminPaymentSettings> {
    const params: HttpParams = HttpUtils.createFormParams({
      apiPublicKey: paymentSettings.apiPublicKey,
      apiSecretKey: paymentSettings.apiSecretKey
    });

    return this.http.put(endpoints.paymentSettings(chapterId), params).pipe(
      map((response: any) => this.mapChapterAdminPaymentSettings(response))
    );
  }    

  updateChapterTexts(chapterId: string, texts: ChapterTexts): Observable<ChapterTexts> {
    const params: HttpParams = HttpUtils.createFormParams({
      registerText: texts.registerText,
      welcomeText: texts.welcomeText
    });

    return this.http.put(endpoints.texts(chapterId), params).pipe(
      map((response: any) => this.mapChapterTexts(response))
    );
  }

  private mapChapterAdminPaymentSettings(response: any): ChapterAdminPaymentSettings {
    return {
      apiPublicKey: response.apiPublicKey,
      apiSecretKey: response.apiSecretKey,
      provider: response.provider
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

  private mapChapterEmailSettings(response: any): ChapterEmailSettings {
    return {
      adminEmailAddress: response.adminEmailAddress,
      contactEmailAddress: response.contactEmailAddress
    };
  }
}
