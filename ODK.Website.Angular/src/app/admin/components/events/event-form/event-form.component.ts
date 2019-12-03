import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { FormControlViewModel } from 'src/app/components/forms/form-control.view-model';
import { FormViewModel } from 'src/app/components/forms/form.view-model';

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
    address: FormControlViewModel
    date: FormControlViewModel,
    description: FormControlViewModel,
    isPublic: FormControlViewModel,
    location: FormControlViewModel,
    name: FormControlViewModel,
    time: FormControlViewModel,
    mapQuery: FormControlViewModel
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
      isPublic: this.formControls.isPublic.value === 'true',
      location: this.formControls.location.value,
      mapQuery: this.formControls.mapQuery.value,
      name: this.formControls.name.value,
      time: this.formControls.time.value
    };

    this.formSubmit.emit(event);
  }

  private buildForm(): void {
    this.formControls = {
      address: {
        helpText: 'Additional location information, if required',
        id: 'address',
        label: 'Address',
        value: this.event ? this.event.address : ''
      },
      date: {
        id: 'date',
        label: 'Date',
        type: 'date',
        validators: {
          required: true
        },
        value: this.datePipe.transform(this.event ? this.event.date : new Date(), 'yyyy-MM-dd')
      },
      description: {
        id: 'description',
        label: 'Description',
        type: 'textarea',
        value: this.event ? this.event.description : ''
      },
      isPublic: {
        id: 'ispublic',
        label: 'Public',
        type: 'checkbox',
        value: this.event && this.event.isPublic ? 'true'  : 'false'
      },
      location: {
        helpText: 'The main description for where the event is happening',
        id: 'location',
        label: 'Location',
        validators: {
          required: true
        },
        value: this.event ? this.event.location : ''
      },
      mapQuery: {
        helpText: 'The search term used if displaying a map. Be as specific as possible',
        id: 'mapquery',
        label: 'Map search',
        value: this.event ? this.event.mapQuery : ''
      },
      name: {
        id: 'name',
        label: 'Name',
        validators: {
          required: true
        },
        value: this.event ? this.event.name : ''
      },
      time: {
        id: 'time',
        label: 'Time',
        value: this.event ? this.event.time : ''
      }
    };

    this.form = {
      buttonText: this.buttonText,
      callback: this.formCallback,
      formControls: [
        this.formControls.name,
        this.formControls.location,
        this.formControls.date,
        this.formControls.time,
        this.formControls.address,
        this.formControls.mapQuery,
        this.formControls.description,
        this.formControls.isPublic
      ]
    };
  }
}
