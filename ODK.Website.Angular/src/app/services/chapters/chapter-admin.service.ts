import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { catchApiError } from '../http/catchApiError';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminMembershipSettings } from 'src/app/core/chapters/chapter-admin-membership-settings';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from './chapter.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { environment } from 'src/environments/environment';
import { HttpStore } from '../http/http-store';
import { HttpUtils } from '../http/http-utils';
import { ServiceResult } from '../service-result';

const baseUrl = `${environment.adminApiBaseUrl}/chapters`;

const endpoints = {
  adminMember: (id: string, memberId: string) => `${baseUrl}/${id}/adminmembers/${memberId}`,
  adminMembers: (id: string) => `${baseUrl}/${id}/adminmembers`,
  chapters: baseUrl,  
  links: (id: string) => `${baseUrl}/${id}/links`,
  membershipSettings: (id: string) => `${baseUrl}/${id}/membership/settings`,
  paymentSettings: (id: string) => `${baseUrl}/${id}/payments/settings`,
  properties: (id: string) => `${baseUrl}/${id}/properties`,
  property: (id: string) => `${baseUrl}/properties/${id}`,
  propertyMoveDown: (id: string) => `${baseUrl}/properties/${id}/movedown`,
  propertyMoveUp: (id: string) => `${baseUrl}/properties/${id}/moveup`,
  question: (id: string) => `${baseUrl}/questions/${id}`,
  questionMoveDown: (id: string) => `${baseUrl}/questions/${id}/movedown`,
  questionMoveUp: (id: string) => `${baseUrl}/questions/${id}/moveup`,
  questions: (id: string) => `${baseUrl}/${id}/questions`,
  subscription: (id: string) => `${baseUrl}/subscriptions/${id}`,
  subscriptions: (id: string) => `${baseUrl}/${id}/subscriptions`,
  texts: (id: string) => `${baseUrl}/${id}/texts`,
};

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminService extends ChapterService {

  constructor(http: HttpClient, store: HttpStore) {
    super(http, store);
  }

  addChapterAdminMember(chapterId: string, memberId: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      memberId: memberId
    });

    return this.http.post(endpoints.adminMembers(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  createChapterProperty(chapterId: string, property: ChapterProperty): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      dataType: property.dataType.toString(),
      helpText: property.helpText,
      hidden: property.hidden ? 'True' : 'False',
      label: property.label,
      name: property.name,
      required: property.required ? 'True' : 'False',
      subtitle: property.subtitle
    });

    return this.http.post(endpoints.properties(chapterId), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    )
  }

  createChapterQuestion(chapterId: string, chapterQuestion: ChapterQuestion): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      answer: chapterQuestion.answer,
      name: chapterQuestion.name
    });

    return this.http.post(endpoints.questions(chapterId), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
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

  deleteChapterProperty(property: ChapterProperty): Observable<void> {
    return this.http.delete(endpoints.property(property.id)).pipe(
      map(() => undefined)
    );
  }

  deleteChapterQuestion(question: ChapterQuestion): Observable<void> {
    return this.http.delete(endpoints.question(question.id)).pipe(
      map(() => undefined)
    );
  }

  deleteChapterSubscription(subscription: ChapterSubscription): Observable<void> {
    return this.http.delete(endpoints.subscription(subscription.id)).pipe(
      map(() => undefined)
    );
  }

  getAdminChapterProperties(chapterId: string): Observable<ChapterProperty[]> {
    return this.http.get(endpoints.properties(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterProperty(x)))
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

  getChapterAdminMembershipSettings(chapterId: string): Observable<ChapterAdminMembershipSettings> {
    return this.http.get(endpoints.membershipSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterAdminMembershipSettings(response))
    );
  }

  getChapterAdminPaymentSettings(chapterId: string): Observable<ChapterAdminPaymentSettings> {
    return this.http.get(endpoints.paymentSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterAdminPaymentSettings(response))
    );
  }  

  getChapterProperty(id: string): Observable<ChapterProperty> {
    return this.http.get(endpoints.property(id)).pipe(
      map((response: any) => this.mapChapterProperty(response))
    );
  }

  getChapterQuestion(id: string): Observable<ChapterQuestion> {
    return this.http.get(endpoints.question(id)).pipe(
      map((response: any) => this.mapChapterQuestion(response))
    );
  }

  getChapterSubscription(id: string): Observable<ChapterSubscription> {
    return this.http.get(endpoints.subscription(id)).pipe(
      map((response: any) => this.mapChapterSubscription(response))
    );
  }

  hasAccess(chapter: Chapter): Observable<boolean> {
    return chapter ? this.getAdminChapters().pipe(
      map((chapters: Chapter[]) => !!chapters.find(x => x.id === chapter.id))
    ) : of(false);
  }

  moveChapterPropertyDown(id: string): Observable<ChapterProperty[]> {
    return this.http.put(endpoints.propertyMoveDown(id), {}).pipe(
      map((response: any) => response.map(x => this.mapChapterProperty(x)))
    );
  }

  moveChapterPropertyUp(id: string): Observable<ChapterProperty[]> {
    return this.http.put(endpoints.propertyMoveUp(id), {}).pipe(
      map((response: any) => response.map(x => this.mapChapterProperty(x)))
    );
  }

  moveChapterQuestionDown(id: string): Observable<ChapterQuestion[]> {
    return this.http.put(endpoints.questionMoveDown(id), {}).pipe(
      map((response: any) => response.map(x => this.mapChapterQuestion(x)))
    );
  }

  moveChapterQuestionUp(id: string): Observable<ChapterQuestion[]> {
    return this.http.put(endpoints.questionMoveUp(id), {}).pipe(
      map((response: any) => response.map(x => this.mapChapterQuestion(x)))
    );
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

  updateChapterAdminMembershipSettings(chapterId: string, settings: ChapterAdminMembershipSettings): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      membershipDisabledAfterDaysExpired: settings.membershipDisabledAfterDaysExpired.toString(),
      trialPeriodMonths: settings.trialPeriodMonths.toString()
    });

    return this.http.put(endpoints.membershipSettings(chapterId), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }    

  updateChapterAdminPaymentSettings(chapterId: string, settings: ChapterAdminPaymentSettings): Observable<ChapterAdminPaymentSettings> {
    const params: HttpParams = HttpUtils.createFormParams({
      apiPublicKey: settings.apiPublicKey,
      apiSecretKey: settings.apiSecretKey
    });

    return this.http.put(endpoints.paymentSettings(chapterId), params).pipe(
      map((response: any) => this.mapChapterAdminPaymentSettings(response))
    );
  }    

  updateChapterLinks(chapterId: string, links: ChapterLinks): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      facebook: links.facebook,
      instagram: links.instagram,
      twitter: links.twitter
    });

    return this.http.put(endpoints.links(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  updateChapterProperty(property: ChapterProperty): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      helpText: property.helpText,
      hidden: property.hidden ? 'True' : 'False',
      label: property.label,
      name: property.name,
      required: property.required ? 'True' : 'False',
      subtitle: property.subtitle
    });

    return this.http.put(endpoints.property(property.id), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  updateChapterQuestion(question: ChapterQuestion): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      answer: question.answer,
      name: question.name
    });

    return this.http.put(endpoints.question(question.id), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
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

    return this.http.put(endpoints.subscription(subscription.id), params).pipe(
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

  private mapChapterAdminMembershipSettings(response: any): ChapterAdminMembershipSettings {
    return {
      membershipDisabledAfterDaysExpired: response.membershipDisabledAfterDaysExpired,
      trialPeriodMonths: response.trialPeriodMonths
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
