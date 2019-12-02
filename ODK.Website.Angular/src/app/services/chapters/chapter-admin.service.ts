import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterService } from './chapter.service';

const baseUrl = `${environment.baseUrl}/admin/chapters`;

const endpoints = {
  chapters: baseUrl,
  paymentSettings: (id: string) => `${baseUrl}/${id}/paymentsettings`
};

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminService extends ChapterService {

  constructor(http: HttpClient) { 
    super(http);
  }  

  getAdminChapters(): Observable<Chapter[]> {
    return this.http.get(endpoints.chapters).pipe(
      map((response: any) => response.map(x => this.mapChapter(x)))
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

  private mapChapterAdminPaymentSettings(response: any): ChapterAdminPaymentSettings {
    return {
      apiPublicKey: response.apiPublicKey,
      apiSecretKey: response.apiSecretKey,
      provider: response.provider
    };
  }
}
