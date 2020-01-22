import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of, Subject, ReplaySubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterMembershipSettings } from 'src/app/core/chapters/chapter-membership-settings';
import { ChapterPaymentSettings } from 'src/app/core/chapters/chapter-payment-settings';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';

const baseUrl = `${environment.apiBaseUrl}/chapters`;

const endpoints = {
  chapterContact: (id: string) => `${baseUrl}/${id}/contact`,  
  chapterLinks: (id: string) => `${baseUrl}/${id}/links`,
  chapterMembershipSettings: (id: string) => `${baseUrl}/${id}/membership/settings`,
  chapterPaymentSettings: (id: string) => `${baseUrl}/${id}/payments/settings`,
  chapterProperties: (id: string) => `${baseUrl}/${id}/properties`,
  chapterPropertyOptions: (id: string) => `${baseUrl}/${id}/propertyOptions`,
  chapterQuestions: (id: string) => `${baseUrl}/${id}/questions`,
  chapters: baseUrl,
  chapterSubscriptions: (id: string) => `${baseUrl}/${id}/subscriptions`,
  chapterTexts: (id: string) => `${baseUrl}/${id}/texts`
}

@Injectable({
  providedIn: 'root'
})
export class ChapterService {

  constructor(protected http: HttpClient) { }

  private activeChapter: Chapter;
  private activeChapterSubject: Subject<Chapter> = new ReplaySubject<Chapter>(1);
  private chapterDetails: Map<string, ChapterTexts> = new Map<string, ChapterTexts>();
  private chapterLinks: Map<string, ChapterLinks> = new Map<string, ChapterLinks>();
  private chapters: Chapter[];

  activeChapterChange(): Observable<Chapter> {
    return this.activeChapterSubject.asObservable();
  }

  contact(chapterId: string, email: string, message: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      emailAddress: email,
      message: message
    });

    return this.http.post(endpoints.chapterContact(chapterId), params).pipe(
      map(() => undefined)
    );
  }

  getActiveChapter(): Chapter {
    return this.activeChapter;
  }

  getChapter(name: string): Observable<Chapter> {
    return this.getChapters().pipe(
      map((chapters: Chapter[]) => chapters.find(x => x.name.toLocaleLowerCase() === name.toLocaleLowerCase()))
    );
  }  

  getChapterById(id: string): Observable<Chapter> {
    return this.getChapters().pipe(
      map((chapters: Chapter[]) => chapters.find(x => x.id === id))
    );
  }

  getChapterLinks(chapterId: string): Observable<ChapterLinks> {
    if (this.chapterLinks.has(chapterId)) {
      return of(this.chapterLinks.get(chapterId));
    }

    return this.http.get(endpoints.chapterLinks(chapterId)).pipe(
      map((response: any) => this.mapChapterLinks(response)),
      tap((links: ChapterLinks) => this.chapterLinks.set(chapterId, links))
    );
  }

  getChapterMembershipSettings(chapterId: string): Observable<ChapterMembershipSettings> {
    return this.http.get(endpoints.chapterMembershipSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterMembershipSettings(response))
    );
  }

  getChapterPaymentSettings(chapterId: string): Observable<ChapterPaymentSettings> {
    return this.http.get(endpoints.chapterPaymentSettings(chapterId)).pipe(
      map((response: any) => this.mapChapterPaymentSettings(response))
    );
  }

  getChapterProperties(chapterId: string): Observable<ChapterProperty[]> {
    return this.http.get(endpoints.chapterProperties(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterProperty(x)))
    );
  }

  getChapterPropertyOptions(chapterId: string): Observable<ChapterPropertyOption[]> {
    return this.http.get(endpoints.chapterPropertyOptions(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterPropertyOption(x)))
    );
  }

  getChapterQuestions(chapterId: string): Observable<ChapterQuestion[]> {
    return this.http.get(endpoints.chapterQuestions(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterQuestion(x)))
    );
  }

  getChapters(): Observable<Chapter[]> {
    if (this.chapters) {
      return of(this.chapters);
    }

    return this.http.get(endpoints.chapters)
      .pipe(
        map((response: any): Chapter[] => response.map(x => this.mapChapter(x))),
        tap((chapters: Chapter[]) => this.chapters = chapters)
      )
  }

  getChapterSubscriptions(chapterId: string): Observable<ChapterSubscription[]> {
    return this.http.get(endpoints.chapterSubscriptions(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapChapterSubscription(x)))
    );
  }

  getChapterTexts(chapterId: string): Observable<ChapterTexts> {
    if (this.chapterDetails.has(chapterId)) {
      return of(this.chapterDetails.get(chapterId));
    }

    return this.http.get(endpoints.chapterTexts(chapterId)).pipe(
      map((response: any) => this.mapChapterTexts(response)),
      tap((details: ChapterTexts) => this.chapterDetails.set(chapterId, details))
    );
  }

  setActiveChapter(chapter: Chapter): void {
    this.activeChapter = chapter;
    this.activeChapterSubject.next(chapter);
  }

  protected mapChapter(response: any): Chapter {
    return {
      bannerImageUrl: response.bannerImageUrl,
      countryId: response.countryId,
      id: response.id,
      name: response.name,
      redirectUrl: response.redirectUrl
    }
  }  

  protected mapChapterProperty(response: any): ChapterProperty {
    return {
      dataType: response.dataTypeId,
      helpText: response.helpText,
      hidden: response.hidden === true,
      id: response.id,
      label: response.label,
      name: response.name,
      required: response.required === true,
      subtitle: response.subtitle
    };
  }
  
  protected mapChapterQuestion(response: any): ChapterQuestion {
    return {
      answer: response.answer,
      id: response.id,
      name: response.name
    };
  }  
  
  protected mapChapterSubscription(response: any): ChapterSubscription {
    return {
      amount: response.amount,
      chapterId: response.chapterId,
      description: response.description,
      id: response.id,
      months: response.months,
      name: response.name,
      title: response.title,
      type: response.type
    };
  }
  
  private mapChapterLinks(response: any): ChapterLinks {
    return {
      facebook: response.facebook,
      instagram: response.instagram,
      twitter: response.twitter
    };
  }

  private mapChapterMembershipSettings(response: any): ChapterMembershipSettings {
    return {
      trialPeriodMonths: response.trialPeriodMonths
    };
  }

  private mapChapterPaymentSettings(response: any): ChapterPaymentSettings {
    return {
      apiPublicKey: response.apiPublicKey,
      provider: response.provider
    };
  }  

  private mapChapterPropertyOption(response: any): ChapterPropertyOption {
    return {
      chapterPropertyId: response.chapterPropertyId,
      freeText: response.freeText === true,
      value: response.value
    };
  }  

  protected mapChapterTexts(response: any): ChapterTexts {
    return {      
      registerText: response.registerText,
      welcomeText: response.welcomeText
    };
  }
}
