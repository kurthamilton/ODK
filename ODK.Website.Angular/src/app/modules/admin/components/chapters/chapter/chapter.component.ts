import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private authenticationService: AuthenticationService) {     
  }

  superAdmin: boolean;

  ngOnInit(): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.superAdmin = token.superAdmin === true;
  }
}
