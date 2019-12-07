import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject, Observable } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-questions',
  templateUrl: './chapter-questions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterQuestionsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  form: FormViewModel;
  questions: ChapterQuestion[];

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    answer: HtmlEditorFormControlViewModel;
    name: TextInputFormControlViewModel;
  };
  
  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();    

    this.loadQuestions().subscribe(() => {
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    const question: ChapterQuestion = {
      answer: this.formControls.answer.value,
      name: this.formControls.name.value
    };

    this.chapterAdminService.createChapterQuestion(this.chapter.id, question).pipe(
      switchMap(() => this.loadQuestions())
    ).subscribe(() => {      
      this.form = null;
      this.changeDetector.detectChanges();

      this.buildForm();
      this.changeDetector.detectChanges();
    })
  }

  private buildForm(): void {
    this.formControls = {
      answer: new HtmlEditorFormControlViewModel({
        id: 'answer',
        label: {
          text: 'Answer'
        },
        validation: {
          required: true
        }
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          text: 'Question'
        },
        validation: {
          required: true
        }
      })
    };

    this.form = {
      buttonText: 'Add',
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.answer
      ]
    };
  }

  private loadQuestions(): Observable<ChapterQuestion[]> {
    return this.chapterAdminService.getChapterQuestions(this.chapter.id).pipe(
      tap((questions: ChapterQuestion[]) => this.questions = questions)
    );
  }
}
