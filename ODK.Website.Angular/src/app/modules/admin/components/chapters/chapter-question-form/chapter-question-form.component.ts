import { Component, ChangeDetectionStrategy, OnChanges, Input, Output, EventEmitter } from '@angular/core';

import { Subject } from 'rxjs';

import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-question-form',
  templateUrl: './chapter-question-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterQuestionFormComponent implements OnChanges {

  @Input() buttonText: string;
  @Input() formCallback: Subject<boolean | string[]>;
  @Input() question: ChapterQuestion;
  @Output() formSubmit: EventEmitter<ChapterQuestion> = new EventEmitter<ChapterQuestion>();

  form: FormViewModel;

  private formControls: {
    answer: HtmlEditorFormControlViewModel;
    name: TextInputFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.question) {
      return;
    }

    this.buildForm();
  }

  onFormSubmit(): void {
    this.question.answer = this.formControls.answer.value;
    this.question.name = this.formControls.name.value;

    this.formSubmit.emit(this.question);
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
        },
        value: this.question ? this.question.answer : ''
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          text: 'Question'
        },
        validation: {
          required: true
        },
        value: this.question ? this.question.name : ''
      })
    };

    this.form = {
      buttons: [
        { text: this.buttonText }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.answer
      ],
      messages: {
        success: this.question.id
          ? 'Question updated'
          : 'Question created'
      }
    };
  }
}
