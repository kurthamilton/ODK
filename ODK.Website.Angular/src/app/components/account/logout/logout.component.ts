import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';

import { appPaths } from 'src/app/routing/app-paths';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogoutComponent implements OnInit {

  constructor(private router: Router,
    private authenticationService: AuthenticationService
  ) {     
  }

  ngOnInit(): void {
    this.authenticationService.logout().subscribe(() => {
      this.router.navigateByUrl(appPaths.home.path);
    });
  }

}
