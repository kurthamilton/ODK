import { Component, OnInit, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { MenuItem } from '../navbar/menu-item';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';

@Component({
  selector: 'app-chapter-menu',
  templateUrl: './chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterMenuComponent implements OnInit, OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService
  ) {
  }

  @Input() chapter: Chapter;

  menuItems: MenuItem[];

  private memberChapterId: string;
  
  ngOnInit(): void {
    this.authenticationService.isAuthenticated().subscribe((token: AuthenticationToken) => {
      this.memberChapterId = token ? token.chapterId : '';
      this.buildMenu();
      this.changeDetector.detectChanges();
    });
  }

  ngOnChanges(): void {
    this.buildMenu();
  }

  private buildMenu(): void {
    if (!this.chapter) {
      this.menuItems = [];
      return;
    }

    this.menuItems = [
      { link: appUrls.events(this.chapter), text: 'Events' }
    ];
    
    if (this.memberChapterId === this.chapter.id) {
      this.menuItems.push({ link: appUrls.members(this.chapter), text: 'Knitwits' });
    }

    this.menuItems.push(...[
      { link: appUrls.blog(this.chapter), text: 'Blog' },
      { link: appUrls.contact(this.chapter), text: 'Contact' },
      { link: appUrls.about(this.chapter), text: 'FAQ' }
    ]);
  }
}
