import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export abstract class RouteGuardService implements CanActivate {

  constructor(private router: Router, private redirectUrl?: string) {
  }

  abstract hasAccess(route?: ActivatedRouteSnapshot): Observable<boolean>;

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean | UrlTree> {    
    return this.hasAccess(route).pipe(
      map((permitted: boolean): boolean | UrlTree => 
        permitted === true ? permitted : this.router.parseUrl(this.redirectUrl || '/'))
    );
  }
}
