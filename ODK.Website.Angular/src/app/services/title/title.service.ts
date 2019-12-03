import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../chapters/chapter.service';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TitleService {

  private _appTitle: string;

  constructor(private title: Title,
    private chapterService: ChapterService
  ) {
    this._appTitle = environment.title;

    this.chapterService.activeChapterChange().subscribe((chapter: Chapter) => {
      this.chapter = chapter;
      this.updateTitle();
    });
  }

  private chapter: Chapter;
  private routeTitle: string;
  private routeTitles: string[] = [];

  getRouteTitle(): string {
    return this.routeTitle;
  }

  setRouteTitle(...titles: string[]): void {
    titles = titles.filter(x => !!x);
    this.routeTitle = titles[titles.length - 1];
    this.routeTitles = titles;
    this.updateTitle();
  }

  private getTitle(): string {
    const parts: string[] = [
      `${this.chapter ? `${this.chapter.name} ` : ''}${this._appTitle}`
    ];

    if (this.routeTitles.length) {
      parts.unshift(...this.routeTitles);
    }

    return parts.join(' | ');
  }

  private updateTitle(): void {
    const title: string = this.getTitle();
    this.title.setTitle(title);
  }
}
