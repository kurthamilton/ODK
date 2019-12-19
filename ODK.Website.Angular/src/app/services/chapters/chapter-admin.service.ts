import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterDetails } from 'src/app/core/chapters/chapter-details';
import { ChapterEmailSettings } from 'src/app/core/chapters/chapter-email-settings';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from './chapter.service';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  chapters: baseUrl,
  details: (id: string) => `${baseUrl}/${id}/details`,
  emailSettings: (id: string) => `${baseUrl}/${id}/emails/settings`,
  paymentSettings: (id: string) => `${baseUrl}/${id}/payments/settings`,
  questions: (id: string) => `${baseUrl}/${id}/Questions`
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

  hasAccess(chapter: Chapter): Observable<boolean> {
    return chapter ? this.getAdminChapters().pipe(
      map((chapters: Chapter[]) => !!chapters.find(x => x.id === chapter.id))
    ) : of(false);
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

  updateChapterDetails(chapterId: string, details: ChapterDetails): Observable<ChapterDetails> {
    const params: HttpParams = HttpUtils.createFormParams({
      welcomeText: details.welcomeText
    });

    return this.http.put(endpoints.details(chapterId), params).pipe(
      map((response: any) => this.mapChapterDetails(response))
    );
  }

  updateChapterEmailSettings(chapterId: string, emailSettings: ChapterEmailSettings): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      adminEmailAddress: emailSettings.adminEmailAddress,
      contactEmailAddress: emailSettings.contactEmailAddress,
      emailApiKey: emailSettings.emailApiKey,
      fromEmailAddress: emailSettings.fromEmailAddress,
      fromEmailName: emailSettings.fromEmailName
    });

    return this.http.put(endpoints.emailSettings(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  private mapChapterAdminPaymentSettings(response: any): ChapterAdminPaymentSettings {
    return {
      apiPublicKey: response.apiPublicKey,
      apiSecretKey: response.apiSecretKey,
      provider: response.provider
    };
  }

  private mapChapterEmailSettings(response: any): ChapterEmailSettings {
    return {
      adminEmailAddress: response.adminEmailAddress,
      contactEmailAddress: response.contactEmailAddress,
      emailApiKey: response.emailApiKey,
      emailProvider: response.emailProvider,
      fromEmailAddress: response.fromEmailAddress,
      fromEmailName: response.fromEmailName
    };
  }
}
