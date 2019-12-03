import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';

import { takeUntil, filter, map, mergeMap } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy  {
  constructor(private router: Router,
    private route: ActivatedRoute,
    private titleService: TitleService,
  ) {
  }

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
  }

  ngOnDestroy(): void {}

  private rootRoute(route: ActivatedRoute): ActivatedRoute {
    while (route.firstChild) {
      route = route.firstChild;
    }
    return route;
  }
}
