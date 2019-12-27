import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';

import { AuthenticationService } from '../services/authentication/authentication.service';
import { AuthenticationToken } from '../core/authentication/authentication-token';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class UnauthenticatedGuardService extends RouteGuardService {

  constructor(router: Router, 
    private authenticationService: AuthenticationService
  ) { 
    super(router);
  }

  hasAccess(): Observable<boolean> {
    return this.authenticationService.authenticationTokenChange().pipe(
      take(1),
      map((token: AuthenticationToken) => !token)
    );
  }
}
