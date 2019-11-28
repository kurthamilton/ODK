import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { Chapter } from 'src/app/core/chapters/chapter';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  chapters: baseUrl
};

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminService {

  constructor(private http: HttpClient) { }

  private activeChapter: Chapter;

  getActiveChapter(): Chapter {
    return this.activeChapter;
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
    this.activeChapter = chapter;
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
