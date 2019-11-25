import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Subject } from 'rxjs';

import { AccountService } from 'src/app/services/account/account.service';
import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
import { DataTypeService } from 'src/app/services/data-types/data-type.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit {
  
  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) {
  }
  
  links: {
    subscription: string
  };
  showChangePasswordModal: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    const authenticationToken: AuthenticationToken = this.authenticationService.getToken();
    const chapterId: string = authenticationToken.chapterId;    

    this.chapterService.getChapterById(chapterId).subscribe((chapter: Chapter) => {
      this.links = {
        subscription: appUrls.subscription(chapter)
      };
      this.changeDetector.detectChanges();
    });    
  }  
  
  onPasswordChanged(): void {
    this.showChangePasswordModal.next(false);
  }

  onShowChangePasswordModal(): void {
    this.showChangePasswordModal.next(true);
  }  
}
