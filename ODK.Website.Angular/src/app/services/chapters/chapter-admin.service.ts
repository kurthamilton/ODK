import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterEmailProviderSettings } from 'src/app/core/chapters/chapter-email-provider-settings';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from './chapter.service';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  adminMember: (id: string, memberId: string) => `${baseUrl}/${id}/adminmembers/${memberId}`,
  adminMembers: (id: string) => `${baseUrl}/${id}/adminmembers`,
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

  addChapterAdminMember(chapterId: string, memberId: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      memberId: memberId
    });

    return this.http.post(endpoints.adminMembers(chapterId), params).pipe(
      map(() => undefined)
    );
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

  getChapterAdminMember(chapterId: string, memberId: string): Observable<ChapterAdminMember> {
    return this.http.get(endpoints.adminMember(chapterId, memberId)).pipe(
      map((response: any) => this.mapChapterAdminMember(response))
    );
  }

  getChapterAdminMembers(chapterId: string): Observable<ChapterAdminMember[]> {
    return this.http.get(endpoints.adminMembers(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterAdminMember(x)))
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

  removeChapterAdminMember(chapterId: string, adminMember: ChapterAdminMember): Observable<void> {
    return this.http.delete(endpoints.adminMember(chapterId, adminMember.memberId)).pipe(
      map(() => undefined)
    );
  }

  updateChapterAdminMember(chapterId: string, adminMember: ChapterAdminMember): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      adminEmailAddress: adminMember.adminEmailAddress,
      receiveContactEmails: adminMember.receiveContactEmails ? 'True' : 'False',
      receiveNewMemberEmails: adminMember.receiveNewMemberEmails ? 'True' : 'False',
      sendEventEmails: adminMember.sendEventEmails ? 'True' : 'False',
      sendNewMemberEmails: adminMember.sendNewMemberEmails ? 'True' : 'False'
    });

    return this.http.put(endpoints.adminMember(chapterId, adminMember.memberId), params).pipe(
      map(() => undefined)
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

    return this.http.put(endpoints.emailProviderSettings(chapterId), params).pipe(
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

  private mapChapterAdminMember(response: any): ChapterAdminMember {
    return {
      adminEmailAddress: response.adminEmailAddress,
      firstName: response.firstName,
      lastName: response.lastName,
      memberId: response.memberId,
      receiveContactEmails: response.receiveContactEmails,
      receiveNewMemberEmails: response.receiveNewMemberEmails,
      sendEventEmails: response.sendEventEmails,
      sendNewMemberEmails: response.sendNewMemberEmails
    };
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
}
