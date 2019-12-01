import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterDetails } from 'src/app/core/chapters/chapter-details';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { environment } from 'src/environments/environment';

const baseUrl = `${environment.baseUrl}/chapters`;

const endpoints = {
  chapterDetails: (id: string) => `${baseUrl}/${id}`,
  chapterLinks: (id: string) => `${baseUrl}/${id}/links`,
  chapterProperties: (id: string) => `${baseUrl}/${id}/properties`,
  chapterPropertyOptions: (id: string) => `${baseUrl}/${id}/propertyOptions`,
  chapters: baseUrl
}

@Injectable({
  providedIn: 'root'
})
export class ChapterService {

  constructor(protected http: HttpClient) { }

  private activeChapter: Chapter;
  private chapterDetails: Map<string, ChapterDetails> = new Map<string, ChapterDetails>();
  private chapterLinks: Map<string, ChapterLinks> = new Map<string, ChapterLinks>();
  private chapters: Chapter[];

  getActiveChapter(): Chapter {
    return this.activeChapter;
  }

  getChapter(name: string): Observable<Chapter> {    
    return this.getChapters().pipe(
      map((chapters: Chapter[]) => chapters.find(x => x.name.toLocaleLowerCase() === name.toLocaleLowerCase()))
    );
  }

  getChapterDetails(chapterId: string): Observable<ChapterDetails> {
    if (this.chapterDetails.has(chapterId)) {
      return of(this.chapterDetails.get(chapterId));
    }

    return this.http.get(endpoints.chapterDetails(chapterId)).pipe(
      map((response: any) => this.mapChapterDetails(response)),
      tap((details: ChapterDetails) => this.chapterDetails.set(chapterId, details))
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

  setActiveChapter(chapter: Chapter): void {
    this.activeChapter = chapter;
  }

  protected mapChapter(response: any): Chapter {
    return {
      countryId: response.countryId,
      id: response.id,
      name: response.name,
      redirectUrl: response.redirectUrl
    }
  }

  private mapChapterDetails(response: any): ChapterDetails {
    return {
      bannerImageUrl: response.bannerImageUrl,      
      welcomeText: response.welcomeText
    };
  }

  private mapChapterLinks(response: any): ChapterLinks {
    return {
      facebook: response.facebook,
      instagram: response.instagram,
      twitter: response.twitter
    };
  }

  private mapChapterProperty(response: any): ChapterProperty {
    return {
      dataType: response.dataTypeId,
      helpText: response.helpText,
      id: response.id,
      name: response.name,
      required: response.required === true,
      subtitle: response.subtitle
    };
  }

  private mapChapterPropertyOption(response: any): ChapterPropertyOption {
    return {
      chapterPropertyId: response.chapterPropertyId,
      freeText: response.freeText === true,
      value: response.value
    };
  }
}
