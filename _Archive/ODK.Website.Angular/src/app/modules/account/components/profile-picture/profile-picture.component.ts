import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { AccountService } from 'src/app/services/account/account.service';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { MemberService } from 'src/app/services/members/member.service';
import { LoadingSpinnerOptions } from 'src/app/modules/shared/components/elements/loading-spinner/loading-spinner-options';

@Component({
  selector: 'app-profile-picture',
  templateUrl: './profile-picture.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfilePictureComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private memberService: MemberService,
    private accountService: AccountService
  ) {
  }

  imageUrl: string;
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true
  };
  updating: boolean;

  private memberId: string;

  ngOnInit(): void {
    const authenticationToken: AuthenticationToken = this.authenticationService.getToken();
    this.memberId = authenticationToken.memberId;
    this.loadImage();
  }

  onRotate(): void {
    this.updating = true;
    this.changeDetector.detectChanges();

    this.accountService.rotateImage().subscribe(() => {
      this.updating = false;
      this.loadImage(true);
      this.changeDetector.detectChanges();
    });
  }

  onUploadPicture(files: FileList): void {
    if (files.length === 0) {
      return;
    }

    this.updating = true;
    this.changeDetector.detectChanges();

    this.accountService.updateImage(files[0]).subscribe(() => {
      this.updating = false;
      this.loadImage(true);
      this.changeDetector.detectChanges();
    });
  }

  private loadImage(forceReload?: boolean): void {
    this.imageUrl = this.memberService.getMemberImageUrl(this.memberId, 250, forceReload);
  }
}
