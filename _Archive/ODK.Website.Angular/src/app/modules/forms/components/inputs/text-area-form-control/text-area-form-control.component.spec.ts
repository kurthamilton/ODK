import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TextAreaComponent as TextAreaFormControlComponent } from './text-area-form-control.component';

describe('TextAreaFormControlComponent', () => {
  let component: TextAreaFormControlComponent;
  let fixture: ComponentFixture<TextAreaFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TextAreaFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TextAreaFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
