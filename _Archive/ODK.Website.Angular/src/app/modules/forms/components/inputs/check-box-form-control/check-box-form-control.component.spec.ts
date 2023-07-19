import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckBoxFormControlComponent } from './check-box-form-control.component';

describe('CheckBoxFormControlComponent', () => {
  let component: CheckBoxFormControlComponent;
  let fixture: ComponentFixture<CheckBoxFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CheckBoxFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CheckBoxFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
