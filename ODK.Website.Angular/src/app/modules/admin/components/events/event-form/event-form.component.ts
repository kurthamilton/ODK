import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges, ChangeDetectorRef } from '@angular/core';

import { Observable } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { Event } from 'src/app/core/events/event';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-event-form',
  templateUrl: './event-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventFormComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private datePipe: DatePipe,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  @Input() buttonText: string;  
  @Input() event: Event;
  @Input() formCallback: Observable<string[]>;
  @Output() formSubmit: EventEmitter<Event> = new EventEmitter<Event>();

  form: FormViewModel;

  private chapter: Chapter;
  private venues: Venue[];

  private formControls: {
    date: TextInputFormControlViewModel;
    description: HtmlEditorFormControlViewModel;
    imageUrl: TextInputFormControlViewModel;
    isPublic: CheckBoxFormControlViewModel;
    name: TextInputFormControlViewModel;
    time: TextInputFormControlViewModel;
    venue: DropDownFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }

    this.chapter = this.chapterAdminService.getActiveChapter();

    this.venueAdminService.getVenues(this.chapter.id).subscribe((venues: Venue[]) => {
      this.venues = venues.sort((a, b) => a.name.localeCompare(b.name));
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onFormSubmit(): void {
    const event: Event = {
      chapterId: this.chapter.id,
      date: new Date(this.formControls.date.value),
      description: this.formControls.description.value,
      id: this.event ? this.event.id : '',
      imageUrl: this.formControls.imageUrl.value,
      isPublic: this.formControls.isPublic.value,
      name: this.formControls.name.value,
      time: this.formControls.time.value,
      venueId: this.formControls.venue.value
    };

    this.formSubmit.emit(event);
  }

  private buildForm(): void {
    this.formControls = {
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
      imageUrl: new TextInputFormControlViewModel({
        id: 'image-url',
        label: {
          text: 'Image URL'
        },
        value: this.event ? this.event.imageUrl : ''
      }),
      isPublic: new CheckBoxFormControlViewModel({
        id: 'ispublic',
        label: {
          text: 'Public'
        },
        value: this.event && this.event.isPublic
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
      }),
      venue: new DropDownFormControlViewModel({
        id: 'venue',
        label: {
          helpText: 'Select an existing venue or create a new venue first on the venues page',
          text: 'Venue'
        },
        options: [
          { text: '', value: '', default: true },
          ...this.venues.map((venue: Venue): DropDownFormControlOption => ({
            text: venue.name,
            value: venue.id
          }))
        ],
        validation: {
          required: true
        },
        value: this.event ? this.event.venueId : ''
      })
    };

    this.form = {
      buttons: [
        { text: this.buttonText }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.isPublic,
        this.formControls.name,
        this.formControls.venue,
        this.formControls.date,
        this.formControls.time,
        this.formControls.description,
        this.formControls.imageUrl
      ]
    };
  }
}
