import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DateInputFormControlComponent } from './date-input-form-control.component';

describe('DateInputFormControlComponent', () => {
  let component: DateInputFormControlComponent;
  let fixture: ComponentFixture<DateInputFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DateInputFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DateInputFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
