import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable } from 'rxjs';
import { take, map } from 'rxjs/operators';

import { adminPaths } from './admin-paths';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { RouteGuardService } from 'src/app/routing/route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticatedGuardService extends RouteGuardService {

  constructor(router: Router, 
    private authenticationService: AuthenticationService
  ) { 
    super(router, `/${adminPaths.login.path}`);
  }

  hasAccess(): Observable<boolean> {
    return this.authenticationService.isAuthenticated().pipe(
      take(1),
      map((token: AuthenticationToken) => !!token)
    );
  }
}
