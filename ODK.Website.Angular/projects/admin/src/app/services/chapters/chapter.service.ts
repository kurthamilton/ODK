import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, Subject, ReplaySubject } from 'rxjs';
import { map, take } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { environment } from 'src/environments/environment';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  chapters: baseUrl
};

@Injectable({
  providedIn: 'root'
})
export class ChapterService {

  constructor(private http: HttpClient) { }

  private activeChapterSubject: Subject<Chapter> = new ReplaySubject<Chapter>(1);

  getActiveChapter(): Observable<Chapter> {
    return this.activeChapterSubject.asObservable()
      .pipe(
        take(1)
      );
  }
  
  getChapter(name: string): Observable<Chapter> {
    return this.getChapters().pipe(
      map((chapters: Chapter[]) => chapters.find(x => x.name.toLocaleLowerCase() === name.toLocaleLowerCase()))
    );
  }

  getChapters(): Observable<Chapter[]> {
    return this.http.get(endpoints.chapters).pipe(
      map((response: any) => response.map(x => this.mapChapter(x)))
    );
  }

  setActiveChapter(chapter: Chapter): void {
    this.activeChapterSubject.next(chapter);
  }

  private mapChapter(response: any): Chapter {
    return {
      countryId: response.countryId,
      id: response.id,
      name: response.name,
      redirectUrl: response.redirectUrl
    };
  }
}
