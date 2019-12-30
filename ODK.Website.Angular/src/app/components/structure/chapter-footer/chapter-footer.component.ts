import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { SocialMediaService } from 'src/app/services/social-media/social-media.service';

@Component({
  selector: 'app-chapter-footer',
  templateUrl: './chapter-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterFooterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private socialMediaService: SocialMediaService
  ) { 
  }

  links: {
    facebook: string;
    facebookName: string;
    instagram: string;
    instagramName: string;    
    twitter: string;
    twitterName: string;
  };

  private chapterLinks: ChapterLinks;

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.chapterService.getChapterLinks(chapter.id).subscribe((links: ChapterLinks) => {   
      this.chapterLinks = links;      
      this.links = {
        facebook: this.socialMediaService.getFacebookAccountLink(this.chapterLinks.facebook),
        facebookName: this.chapterLinks.facebook,
        instagram: this.socialMediaService.getInstagramAccountLink(this.chapterLinks.instagram),
        instagramName: this.chapterLinks.instagram,
        twitter: this.socialMediaService.getTwitterAccountLink(this.chapterLinks.twitter),
        twitterName: this.chapterLinks.twitter
      };
      this.changeDetector.detectChanges();
    });
  }

}
