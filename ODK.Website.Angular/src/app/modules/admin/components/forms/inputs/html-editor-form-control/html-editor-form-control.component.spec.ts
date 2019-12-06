import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HtmlEditorFormControlComponent } from './html-editor-form-control.component';

describe('HtmlEditorFormControlComponent', () => {
  let component: HtmlEditorFormControlComponent;
  let fixture: ComponentFixture<HtmlEditorFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HtmlEditorFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HtmlEditorFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
