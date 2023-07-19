import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { GoogleMapsTextInputFormControlViewModel } from '../../forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';
import { Venue } from 'src/app/core/venues/venue';

@Component({
  selector: 'app-venue-form',
  templateUrl: './venue-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenueFormComponent implements OnChanges {

  @Input() buttonText: string;
  @Input() chapter: Chapter;
  @Input() venue: Venue;
  @Input() formCallback: Observable<string[]>;
  @Output() formSubmit: EventEmitter<Venue> = new EventEmitter<Venue>();

  form: FormViewModel;

  private formControls: {
    address: TextInputFormControlViewModel;
    mapQuery: GoogleMapsTextInputFormControlViewModel;
    name: TextInputFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.chapter) {
      return;
    }

    this.buildForm();
  }

  onFormSubmit(): void {
    const venue: Venue = {
      address: this.formControls.address.value,
      chapterId: this.chapter.id,
      id: this.venue ? this.venue.id : '',
      mapQuery: this.formControls.mapQuery.value,
      name: this.formControls.name.value
    };

    this.formSubmit.emit(venue);
  }

  private buildForm(): void {
    this.formControls = {
      address: new TextInputFormControlViewModel({
        id: 'address',
        label: {
          helpText: 'Additional location information, if required',
          text: 'Address'
        },
        value: this.venue ? this.venue.address : ''
      }),
      mapQuery: new GoogleMapsTextInputFormControlViewModel({
        id: 'mapquery',
        label: {
          helpText: 'The search term used if displaying a map. Be as specific as possible',
          text: 'Map search'
        },
        value: this.venue ? this.venue.mapQuery : ''
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          text: 'Name'
        },
        validation: {
          required: true
        },
        value: this.venue ? this.venue.name : ''
      })
    };

    this.form = {
      buttons: [
        { text: this.buttonText }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.address,
        this.formControls.mapQuery,
      ]
    };
  }
}
