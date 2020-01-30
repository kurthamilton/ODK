import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges, Output, EventEmitter } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { NumberInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/number-input-form-control/number-input-form-control.view-model';
import { SubscriptionType } from 'src/app/core/account/subscription-type';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-subscription-form',
  templateUrl: './subscription-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionFormComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private countryService: CountryService
  ) {
  }

  @Input() formCallback: Observable<boolean | string[]>;
  @Input() subscription: ChapterSubscription;
  @Output() formSubmit: EventEmitter<ChapterSubscription> = new EventEmitter<ChapterSubscription>();

  form: FormViewModel;

  private chapter: Chapter;
  private country: Country;
  private formControls: {
    amount: NumberInputFormControlViewModel;
    description: HtmlEditorFormControlViewModel;
    months: NumberInputFormControlViewModel;
    name: TextInputFormControlViewModel;
    title: TextInputFormControlViewModel;
    type: DropDownFormControlViewModel;
  };

  ngOnChanges(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.countryService.getCountry(this.chapter.countryId).subscribe((country: Country) => {
      this.country = country;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onFormSubmit(): void {
    const subscription: ChapterSubscription = {
      amount: this.formControls.amount.value,
      chapterId: this.chapter.id,
      description: this.formControls.description.value,
      id: this.subscription ? this.subscription.id : '',
      months: this.formControls.months.value,
      name: this.formControls.name.value,
      title: this.formControls.title.value,
      type: parseInt(this.formControls.type.value, 10)
    };
    this.formSubmit.emit(subscription);
  }

  private buildForm(): void {
    this.formControls = {
      amount: new NumberInputFormControlViewModel({
        id: 'amount',
        label: {
          text: `Amount`
        },
        min: 0,
        prefix: {
          text: this.country.currencySymbol
        },
        step: 0.01,
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.amount : null
      }),
      description: new HtmlEditorFormControlViewModel({
        id: 'description',
        label: {
          text: 'Description'
        },
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.description : ''
      }),
      months: new NumberInputFormControlViewModel({
        id: 'months',
        label: {
          text: 'Duration (months)'
        },
        min: 1,
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.months : null
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          helpText: 'How to refer to this subscription in the system',
          text: 'Name'
        },
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.name : ''
      }),
      title: new TextInputFormControlViewModel({
        id: 'title',
        label: {
          text: 'Title'
        },
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.title : ''
      }),
      type: new DropDownFormControlViewModel({
        id: 'type',
        label: {
          text: 'Type'
        },
        options: [
          { text: 'Full', value: SubscriptionType.Full.toString() },
          { text: 'Partial', value: SubscriptionType.Partial.toString() }
        ],
        validation: {
          required: true
        },
        value: this.subscription ? this.subscription.type.toString() : SubscriptionType.Full.toString()
      })
    };

    this.form = {
      buttons: [
        { text: this.subscription ? 'Update' : 'Create' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.type,
        this.formControls.title,
        this.formControls.amount,
        this.formControls.months,
        this.formControls.description
      ],
      messages: {
        success: 'Updated'
      }
    };
  }
}
