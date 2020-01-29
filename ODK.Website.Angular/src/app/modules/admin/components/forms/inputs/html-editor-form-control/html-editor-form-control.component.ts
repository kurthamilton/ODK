import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';

import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';
import { takeUntil } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { MediaAdminService } from 'src/app/services/media/media-admin.service';
import { UploadAdapter } from './upload-adapter';

@Component({
  selector: 'app-html-editor-form-control',
  templateUrl: './html-editor-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HtmlEditorFormControlComponent extends InputBase implements OnInit, OnDestroy {

  constructor(changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private mediaAdminService: MediaAdminService
  ) {
    super(changeDetector);
  }

  Editor = ClassicEditor;

  isInvalid = false;
  isValid = false;
  sourceMode = false;

  private chapter: Chapter;
  private submitted = false;

  ngOnInit(): void {    
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.validateForm.pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe(() => {
      this.submitted = true;
      this.validate();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {}

  onChange() {
    this.control.markAsTouched();
    super.onValidate();
    this.validate();
    this.changeDetector.detectChanges();
  }

  onReady(editor): void {
    editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
      return new UploadAdapter(loader, this.chapter, this.mediaAdminService);
    };
  }

  onToggleMode(): void {
    this.sourceMode = !this.sourceMode;
    this.changeDetector.detectChanges();
  }

  private validate(): void {
    if (!this.submitted) {
      return;
    }

    this.isInvalid = this.control.invalid;
    this.isValid = this.control.valid;
  }
}
