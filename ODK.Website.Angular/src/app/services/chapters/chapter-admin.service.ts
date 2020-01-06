import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { catchApiError } from '../http/catchApiError';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from './chapter.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { ServiceResult } from '../service-result';

const baseUrl = `${environment.apiBaseUrl}/admin/chapters`;

const endpoints = {
  adminMember: (id: string, memberId: string) => `${baseUrl}/${id}/adminmembers/${memberId}`,
  adminMembers: (id: string) => `${baseUrl}/${id}/adminmembers`,
  chapters: baseUrl,  
  paymentSettings: (id: string) => `${baseUrl}/${id}/payments/settings`,
  questions: (id: string) => `${baseUrl}/${id}/questions`,
  subscription: (id: string, subscriptionId: string) => `${baseUrl}/${id}/subscriptions/${subscriptionId}`,
  subscriptions: (id: string) => `${baseUrl}/${id}/subscriptions`,
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

  createChapterSubscription(chapterId: string, subscription: ChapterSubscription): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      amount: subscription.amount.toString(),
      description: subscription.description,
      months: subscription.months.toString(),
      name: subscription.name,
      title: subscription.title,
      type: subscription.type.toString()
    });

    return this.http.post(endpoints.subscriptions(chapterId), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  deleteChapterSubscription(subscription: ChapterSubscription): Observable<void> {
    return this.http.delete(endpoints.subscription(subscription.chapterId, subscription.id)).pipe(
      map(() => undefined)
    );
  }

  getAdminChapters(): Observable<Chapter[]> {
    return this.http.get(endpoints.chapters).pipe(
      map((response: any) => response.map(x => this.mapChapter(x)))
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

  getChapterSubscription(id: string, chapterId: string): Observable<ChapterSubscription> {
    return this.http.get(endpoints.subscription(chapterId, id)).pipe(
      map((response: any) => this.mapChapterSubscription(response))
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
      sendNewMemberEmails: adminMember.sendNewMemberEmails ? 'True' : 'False'
    });

    return this.http.put(endpoints.adminMember(chapterId, adminMember.memberId), params).pipe(
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
  
  updateChapterSubscription(subscription: ChapterSubscription): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      amount: subscription.amount.toString(),
      description: subscription.description,
      months: subscription.months.toString(),
      name: subscription.name,
      title: subscription.title,
      type: subscription.type.toString()
    });

    return this.http.put(endpoints.subscription(subscription.chapterId, subscription.id), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
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
      fullName: `${response.firstName} ${response.lastName}`,
      lastName: response.lastName,
      memberId: response.memberId,
      receiveContactEmails: response.receiveContactEmails,
      receiveNewMemberEmails: response.receiveNewMemberEmails,
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
}
