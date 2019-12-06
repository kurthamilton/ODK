import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { Event } from 'src/app/core/events/event';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { GoogleMapsTextInputFormControlViewModel } from '../../forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-event-form',
  templateUrl: './event-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventFormComponent implements OnChanges {

  constructor(private datePipe: DatePipe) {
  }

  @Input() buttonText: string;
  @Input() chapter: Chapter;
  @Input() event: Event;
  @Input() formCallback: Observable<string[]>;
  @Output() formSubmit: EventEmitter<Event> = new EventEmitter<Event>();

  form: FormViewModel;

  private formControls: {
    address: TextInputFormControlViewModel;
    date: TextInputFormControlViewModel;
    description: HtmlEditorFormControlViewModel;
    isPublic: CheckBoxFormControlViewModel;
    location: TextInputFormControlViewModel;
    mapQuery: GoogleMapsTextInputFormControlViewModel;
    name: TextInputFormControlViewModel;
    time: TextInputFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.chapter) {
      return;
    }

    this.buildForm();
  }

  onFormSubmit(): void {
    const event: Event = {
      address: this.formControls.address.value,
      chapterId: this.chapter.id,
      date: new Date(this.formControls.date.value),
      description: this.formControls.description.value,
      id: this.event ? this.event.id : '',
      imageUrl: '',
      isPublic: this.formControls.isPublic.value,
      location: this.formControls.location.value,
      mapQuery: this.formControls.mapQuery.value,
      name: this.formControls.name.value,
      time: this.formControls.time.value
    };

    this.formSubmit.emit(event);
  }

  private buildForm(): void {
    this.formControls = {
      address: new TextInputFormControlViewModel({
        id: 'address',
        label: {
          helpText: 'Additional location information, if required',
          text: 'Address'
        },
        value: this.event ? this.event.address : ''
      }),
      date: new TextInputFormControlViewModel({
        id: 'date',
        inputType: 'date',
        label: {
          text: 'Date'
        },
        validation: {
          required: true
        },
        value: this.datePipe.transform(this.event ? this.event.date : new Date(), 'yyyy-MM-dd')
      }),
      description: new HtmlEditorFormControlViewModel({
        id: 'description',
        label: {
          text: 'Description'
        },
        value: this.event ? this.event.description : ''
      }),
      isPublic: new CheckBoxFormControlViewModel({
        id: 'ispublic',
        label: {
          text: 'Public'
        },
        value: this.event && this.event.isPublic
      }),
      location: new TextInputFormControlViewModel({
        id: 'location',
        label: {
          helpText: 'The main description for where the event is happening',
          text: 'Location'
        },
        validation: {
          required: true
        },
        value: this.event ? this.event.location : ''
      }),
      mapQuery: new GoogleMapsTextInputFormControlViewModel({
        id: 'mapquery',
        label: {
          helpText: 'The search term used if displaying a map. Be as specific as possible',
          text: 'Map search'
        },
        value: this.event ? this.event.mapQuery : ''
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          text: 'Name'
        },
        validation: {
          required: true
        },
        value: this.event ? this.event.name : ''
      }),
      time: new TextInputFormControlViewModel({
        id: 'time',
        label: {
          text: 'Time'
        },
        value: this.event ? this.event.time : ''
      })
    };

    this.form = {
      buttonText: this.buttonText,
      callback: this.formCallback,
      controls: [
        this.formControls.isPublic,
        this.formControls.name,
        this.formControls.location,
        this.formControls.date,
        this.formControls.time,
        this.formControls.description,
        this.formControls.address,
        this.formControls.mapQuery,
      ]
    };
  }
}
