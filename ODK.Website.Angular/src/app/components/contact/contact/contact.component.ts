import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { TextAreaFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-area-form-control/text-area-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactComponent implements OnInit, OnDestroy {
  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {
  }

  form: FormViewModel;
  links: {
    about: string;
  }
  submitted: boolean;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    email: TextInputFormControlViewModel;
    message: TextAreaFormControlViewModel;
  };
  
  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    
    this.links = {
      about: appUrls.about(this.chapter)
    };

    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onAlertClose(): void {
    this.buildForm();
    this.submitted = false;
    this.changeDetector.detectChanges();
  }

  onFormSubmit(): void {
    this.chapterService.contact(this.chapter.id, this.formControls.email.value, this.formControls.message.value).subscribe(() => {
      this.form = null;
      this.submitted = true;
      this.formCallback.next();
      this.changeDetector.detectChanges();
    });
  }
  
  private buildForm(): void {
    this.formControls = {
      email: new TextInputFormControlViewModel({
        id: 'email',
        label: {
          text: 'Email'
        },
        validation: {
          required: true
        }
      }),
      message: new TextAreaFormControlViewModel({
        id: 'message',
        label: {
          text: 'Message'
        },
        rows: 5,
        validation: {
          required: true
        }
      })
    };

    this.form = {
      buttonText: 'Send',
      callback: this.formCallback,
      controls: [
        this.formControls.email,
        this.formControls.message
      ]
    };
  }
}
