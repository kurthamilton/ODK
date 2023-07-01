import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-social-links',
  templateUrl: './chapter-social-links.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSocialLinksComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  form: FormViewModel;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    facebook: TextInputFormControlViewModel;
    instagram: TextInputFormControlViewModel;
    twitter: TextInputFormControlViewModel;
  };
  private links: ChapterLinks;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterLinks(this.chapter.id).subscribe((links: ChapterLinks) => {
      this.links = links;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.links.facebook = this.formControls.facebook.value;
    this.links.instagram = this.formControls.instagram.value;
    this.links.twitter = this.formControls.twitter.value;

    this.chapterAdminService.updateChapterLinks(this.chapter.id, this.links).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      facebook: new TextInputFormControlViewModel({
        id: 'facebook',
        label: {
          text: 'Facebook'
        },
        prefix: {
          icon: 'fa-facebook-f',
          text: ''
        },
        value: this.links.facebook
      }),
      instagram: new TextInputFormControlViewModel({
        id: 'instagram',
        label: {
          text: 'Instagram'
        },
        prefix: {
          icon: 'fa-instagram',
          text: ''
        },
        value: this.links.instagram
      }),
      twitter: new TextInputFormControlViewModel({
        id: 'twitter',
        label: {
          text: 'Twitter'
        },
        prefix: {
          icon: 'fa-twitter',
          text: ''
        },
        value: this.links.twitter
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.facebook,
        this.formControls.instagram,
        this.formControls.twitter
      ],
      messages: {
        success: 'Updated'
      }
    };
  }
}
