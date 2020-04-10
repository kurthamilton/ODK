import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { SocialMediaAdminService } from 'src/app/services/social-media/social-media-admin.service';
import { Chapter } from 'src/app/core/chapters/chapter';

@Component({
  selector: 'app-instagram',
  templateUrl: './instagram.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InstagramComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private socialMediaAdminService: SocialMediaAdminService
  ) {
  }

  code: string;
  loginMessage: string;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
  }

  onLogin(): void {
    this.socialMediaAdminService.instagramLogin(this.chapter.id).subscribe(result => {
      this.loginMessage = result.messages?.length ? result.messages[0] : '';
      this.changeDetector.detectChanges();
    });
  }

  onSendVerifyCode(): void {
    this.socialMediaAdminService.instagramSendVerifyCode(this.chapter.id, this.code).subscribe();
  }

  onTriggerVerifyCode(): void {
    this.socialMediaAdminService.instagramTriggerVerifyCode(this.chapter.id).subscribe();
  }
}
