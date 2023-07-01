import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TextInputFormControlComponent } from './text-input-form-control.component';

describe('TextInputComponent', () => {
  let component: TextInputFormControlComponent;
  let fixture: ComponentFixture<TextInputFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TextInputFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TextInputFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
