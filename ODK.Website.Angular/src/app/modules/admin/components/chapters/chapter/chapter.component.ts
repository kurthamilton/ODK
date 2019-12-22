import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { NgbTabChangeEvent } from '@ng-bootstrap/ng-bootstrap';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthenticationService) {
  }

  activeTab: string;
  superAdmin: boolean;

  ngOnInit(): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.superAdmin = token.superAdmin === true;
    this.activeTab = this.route.snapshot.queryParamMap.get('tab');
  }

  onTabChange(e: NgbTabChangeEvent): void {
    this.router.navigate([ this.router.url.split('?')[0] ], {
      queryParams: {
        tab: e.nextId
      }
    });
  }
}
