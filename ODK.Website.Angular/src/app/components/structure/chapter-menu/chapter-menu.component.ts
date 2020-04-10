import { Component, OnInit, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { MenuItem } from '../../../core/menus/menu-item';

@Component({
  selector: 'app-chapter-menu',
  templateUrl: './chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterMenuComponent implements OnInit, OnChanges, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService
  ) {
  }

  @Input() chapter: Chapter;

  menuItems: MenuItem[];

  private memberChapterId: string;

  ngOnInit(): void {
    this.authenticationService.authenticationTokenChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((token: AuthenticationToken) => {
      this.memberChapterId = token ? token.chapterId : '';
      this.buildMenu();
      this.changeDetector.detectChanges();
    });
  }

  ngOnChanges(): void {
    this.buildMenu();
  }

  ngOnDestroy(): void {}

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
      { link: appUrls.contact(this.chapter), text: 'Contact' },
      { link: appUrls.about(this.chapter), text: 'About' }
    ]);
  }
}
