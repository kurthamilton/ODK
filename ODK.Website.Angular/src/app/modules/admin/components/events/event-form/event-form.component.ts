import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { CheckBoxViewModel } from 'src/app/modules/forms/components/inputs/check-box/check-box.view-model';
import { DynamicFormViewModel } from 'src/app/modules/forms/components/dynamic-form.view-model';
import { Event } from 'src/app/core/events/event';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { TextAreaViewModel } from 'src/app/modules/forms/components/inputs/text-area/text-area.view-model';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

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

  dynamicForm: DynamicFormViewModel;
  // form: FormViewModel;

  private dynamicFormControls: {
    address: TextInputViewModel;
    date: TextInputViewModel;
    description: TextAreaViewModel;
    isPublic: CheckBoxViewModel;
    location: TextInputViewModel;
    mapQuery: TextInputViewModel;
    name: TextInputViewModel;
    time: TextInputViewModel;
  };

  private formControlsX: {
    address: FormControlViewModel;
    date: FormControlViewModel;
    description: FormControlViewModel;
    isPublic: FormControlViewModel;
    location: FormControlViewModel;
    mapQuery: FormControlViewModel;
    name: FormControlViewModel;
    time: FormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.chapter) {
      return;
    }

    this.buildForm();
  }

  onFormSubmit(): void {
    const event: Event = {
      address: this.dynamicFormControls.address.value,
      chapterId: this.chapter.id,
      date: new Date(this.dynamicFormControls.date.value),
      description: this.dynamicFormControls.description.value,
      id: this.event ? this.event.id : '',
      imageUrl: '',
      isPublic: this.dynamicFormControls.isPublic.value,
      location: this.dynamicFormControls.location.value,
      mapQuery: this.dynamicFormControls.mapQuery.value,
      name: this.dynamicFormControls.name.value,
      time: this.dynamicFormControls.time.value
    };

    this.formSubmit.emit(event);
  }

  private buildForm(): void {
    this.dynamicFormControls = {
      address: new TextInputViewModel({        
        id: 'address',
        label: {
          helpText: 'Additional location information, if required',
          text: 'Address'
        },
        value: this.event ? this.event.address : ''
      }),
      date: new TextInputViewModel({
        id: 'date',
        inputType: 'date',
        label: {
          text: 'Date'
        },
        validators: {
          required: true
        },
        value: this.datePipe.transform(this.event ? this.event.date : new Date(), 'yyyy-MM-dd')
      }),
      description: new TextAreaViewModel({
        id: 'description',
        label: {
          text: 'Description'
        },
        value: this.event ? this.event.description : ''
      }),
      isPublic: new CheckBoxViewModel({
        id: 'ispublic',
        label: {
          text: 'Public'
        },
        value: this.event && this.event.isPublic
      }),
      location: new TextInputViewModel({        
        id: 'location',
        label: {
          helpText: 'The main description for where the event is happening',
          text: 'Location'
        },
        validators: {
          required: true
        },
        value: this.event ? this.event.location : ''
      }),
      mapQuery: new TextInputViewModel({        
        id: 'mapquery',
        label: {
          helpText: 'The search term used if displaying a map. Be as specific as possible',
          text: 'Map search'
        },
        value: this.event ? this.event.mapQuery : ''
      }),
      name: new TextInputViewModel({
        id: 'name',
        label: {
          text: 'Name'
        },
        validators: {
          required: true
        },
        value: this.event ? this.event.name : ''
      }),
      time: new TextInputViewModel({
        id: 'time',
        label: {
          text: 'Time'
        },
        value: this.event ? this.event.time : ''
      })
    };

    this.formControlsX   = {
      address: {        
        id: 'address',
        label: {
          helpText: 'Additional location information, if required',
          text: 'Address'
        },
        value: this.event ? this.event.address : ''
      },
      date: {
        id: 'date',
        label: {
          text: 'Date'
        },
        type: 'date',
        validators: {
          required: true
        },
        value: this.datePipe.transform(this.event ? this.event.date : new Date(), 'yyyy-MM-dd')
      },
      description: {
        id: 'description',
        label: {
          text: 'Description'
        },
        type: 'textarea',
        value: this.event ? this.event.description : ''
      },
      isPublic: {
        id: 'ispublic',
        label: {
          text: 'Public'
        },
        type: 'checkbox',
        value: this.event && this.event.isPublic ? 'true' : ''
      },
      location: {        
        id: 'location',
        label: {
          helpText: 'The main description for where the event is happening',
          text: 'Location'
        },
        validators: {
          required: true
        },
        value: this.event ? this.event.location : ''
      },
      mapQuery: {        
        id: 'mapquery',
        label: {
          helpText: 'The search term used if displaying a map. Be as specific as possible',
          text: 'Map search'
        },
        value: this.event ? this.event.mapQuery : ''
      },
      name: {
        id: 'name',
        label: {
          text: 'Name'
        },
        validators: {
          required: true
        },
        value: this.event ? this.event.name : ''
      },
      time: {
        id: 'time',
        label: {
          text: 'Time'
        },
        value: this.event ? this.event.time : ''
      }
    };

    this.dynamicForm = {
      buttonText: this.buttonText,
      callback: this.formCallback,
      controls: [
        this.dynamicFormControls.name,
        this.dynamicFormControls.location,
        this.dynamicFormControls.date,
        this.dynamicFormControls.time,
        this.dynamicFormControls.address,
        this.dynamicFormControls.mapQuery,
        this.dynamicFormControls.description,
        this.dynamicFormControls.isPublic
      ]
    };
/*
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
    */
  }
}
