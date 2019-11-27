import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, ChangeDetectorRef, Injector } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';
import { takeUntil, delay } from 'rxjs/operators';

import { adminPaths } from '../../routing/admin-paths';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { RouteGuardService } from 'src/app/routing/route-guard.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit, OnDestroy {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private injector: Injector,
    private authenticationService: AuthenticationService
  ) {

  }

  title = 'admin';
  
  private authenticated = false;
  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnInit(): void {
    // this.subscribeAuthChanges();
  }

  ngOnDestroy(): void {
    this.destroyed.next({});
  }

  private runAuthGuard(): void {
    if (!this.route.root.children.length) {
      return;
    }

    const route = this.route.root.children[0];
    if (!route.snapshot.routeConfig || !route.snapshot.routeConfig.canActivate) {
      return;
    }

    const guard = route.snapshot.routeConfig.canActivate[0];
    const routeGuard: RouteGuardService = this.injector.get<RouteGuardService>(guard);
    routeGuard.canActivate(route.snapshot);
  }

  private subscribeAuthChanges(): void {
    // re-run auth guard after auth changes
    this.authenticationService.isAuthenticated()
      .pipe(
        takeUntil(this.destroyed),
        // using delay is a workaround for a problem that arises when navigation occurs before change detection in runAuthGuard()
        delay(0)
      )
      .subscribe((token: AuthenticationToken) => {
        console.log('auth change');
        if (this.authenticated === true && !token) {
          console.log('logged out');
          this.authenticated = false;
          this.router.navigateByUrl(adminPaths.login.path);
          return;
        }

        console.log('run auth guard');
        this.authenticated = !!token;
        this.runAuthGuard();
        this.changeDetector.detectChanges();
      });
  }
}
