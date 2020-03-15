import { Component, OnInit, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';

import { takeUntil, filter, map, mergeMap } from 'rxjs/operators';

import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy  {
  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private titleService: TitleService,
    private chapterService: ChapterService
  ) {
  }

  ready = false;

  ngOnInit(): void {
    this.router.events.pipe(
      takeUntil(componentDestroyed(this)),
      filter((event) => event instanceof NavigationEnd),
      map(() => this.rootRoute(this.route)),
      filter((route: ActivatedRoute) => route.outlet === 'primary'),
      mergeMap((route: ActivatedRoute) => route.data)
    ).subscribe((event: {[name: string]: any}) => {
      this.titleService.setRouteTitle(event['title']);
    });

    this.chapterService.getChapters().subscribe(() => {
      this.ready = true;
      this.changeDetector.detectChanges();
    });    
  }

  ngOnDestroy(): void {}

  private rootRoute(route: ActivatedRoute): ActivatedRoute {
    while (route.firstChild) {
      route = route.firstChild;
    }
    return route;
  }
}
