import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { SocialMediaImage } from 'src/app/core/social-media/social-media-image';

const baseUrl = `${environment.baseUrl}/socialmedia`;

const endpoints = {
  instagram: (chapterId: string, max: number) => `${baseUrl}/instagram?chapterId=${chapterId}&max=${max}`
};

@Injectable({
  providedIn: 'root'
})
export class SocialMediaService {

  constructor(private http: HttpClient) { }

  getChapterInstagramImages(chapterId: string, max: number): Observable<SocialMediaImage[]> {
    return this.http.get(endpoints.instagram(chapterId, max)).pipe(
      map((response: any) => response.map(x => this.mapSocialMediaImage(x))),
      catchError(() => of([]))
    );
  }

  private mapSocialMediaImage(response: any): SocialMediaImage {
    return {
      caption: response.caption,
      imageUrl: response.imageUrl,
      url: response.url
    };
  }
}
