import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Observable } from 'rxjs';
import { concatMap, tap } from 'rxjs/operators';

import { AccountService } from 'src/app/services/account/account.service';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-profile-picture',
  templateUrl: './profile-picture.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfilePictureComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private memberService: MemberService,
    private accountService: AccountService
  ) { 
  }

  imageData: string;
  
  private memberId: string;

  ngOnInit(): void {
    const authenticationToken: AuthenticationToken = this.authenticationService.getToken();
    this.memberId = authenticationToken.memberId;
        
    this.loadImage().subscribe();
  }

  onUploadPicture(files: FileList): void {
    if (files.length === 0) {
      return;
    }
 
    this.accountService.updateImage(files[0]).pipe(
      concatMap(() => this.loadImage())
    ).subscribe();  
  }

  private loadImage(): Observable<{}> {
    return this.memberService.getMemberImage(this.memberId, null).pipe(
      tap((imageData: string) => {
        this.imageData = imageData;
        this.changeDetector.detectChanges();
      })
    )
  }
}
