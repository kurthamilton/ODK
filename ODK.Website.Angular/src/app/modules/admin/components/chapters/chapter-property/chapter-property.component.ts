import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { DataType } from 'src/app/core/data-types/data-type';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-property',
  templateUrl: './chapter-property.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPropertyComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;
  property: ChapterProperty;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    dataType: DropDownFormControlViewModel;
    helpText: TextInputFormControlViewModel;    
    label: TextInputFormControlViewModel;
    name: TextInputFormControlViewModel;    
    required: CheckBoxFormControlViewModel;
    subtitle: TextInputFormControlViewModel;
  };

  private propertyOptions: ChapterPropertyOption[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    const propertyId: string = this.route.snapshot.paramMap.get(adminPaths.chapter.properties.property.params.id);

    forkJoin([
      this.chapterAdminService.getChapterProperties(this.chapter.id).pipe(
        tap((properties: ChapterProperty[]) => this.property = properties.find(x => x.id === propertyId))
      ),
      this.chapterAdminService.getChapterPropertyOptions(this.chapter.id).pipe(
        tap((propertyOptions: ChapterPropertyOption[]) => this.propertyOptions = propertyOptions.filter(x => x.chapterPropertyId === propertyId))
      )
    ]).subscribe(() => {
      if (!this.property) {
        this.router.navigateByUrl(adminUrls.chapterProperties(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.chapterProperties(this.chapter), text: 'Properties' }
      ];

      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {

  }

  private buildForm(): void {
    this.formControls = {
      dataType: new DropDownFormControlViewModel({
        id: 'data-type',
        label: {
          text: 'Data type'
        },
        options: [
          { text: 'Text', value: DataType.Text.toString() },
          { text: 'Long text', value: DataType.LongText.toString() },
          { text: 'Url', value: DataType.Url.toString() },
          { text: 'Drop down', value: DataType.DropDown.toString() }
        ],
        validation: {
          required: true
        },
        value: this.property.dataType.toString()
      }),
      helpText: new TextInputFormControlViewModel({
        id: 'help-text',
        label: {
          text: 'Help text'
        },
        value: this.property.helpText
      }),
      label: new TextInputFormControlViewModel({
        id: 'label',
        label: {
          text: 'Label'
        },
        validation: {
          required: true
        },
        value: this.property.label
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          helpText: 'How this property is referred to in new member emails',
          text: 'Name'
        },
        validation: {
          pattern: '\w+',
          required: true
        },
        value: this.property.name
      }),
      required: new CheckBoxFormControlViewModel({
        id: 'required',
        label: {
          text: 'Required'
        },
        value: this.property.required
      }),
      subtitle: new TextInputFormControlViewModel({
        id: 'subtitle',
        label: {
          text: 'Subtitle'
        },
        value: this.property.subtitle
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.name,
        this.formControls.label,
        this.formControls.dataType,
        this.formControls.required,
        this.formControls.helpText,
        this.formControls.subtitle
      ]
    };
  }
}
