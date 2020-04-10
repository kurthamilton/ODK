import { Injectable } from '@angular/core';
import { Router, ActivatedRouteSnapshot } from '@angular/router';

import { Observable, of } from 'rxjs';

import { AuthenticationService } from '../services/authentication/authentication.service';
import { AuthenticationToken } from '../core/authentication/authentication-token';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterSuperAdminGuardService extends RouteGuardService {

  constructor(
    router: Router,
    private authenticationService: AuthenticationService
  ) {
    super(router);
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const token: AuthenticationToken = this.authenticationService.getToken();
    return of(!!token && token.superAdmin === true);
  }
}
