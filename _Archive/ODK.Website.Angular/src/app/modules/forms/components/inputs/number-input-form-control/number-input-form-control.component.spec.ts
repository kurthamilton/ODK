import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { NumberInputFormControlComponent } from './number-input-form-control.component';

describe('NumberInputFormControlComponent', () => {
  let component: NumberInputFormControlComponent;
  let fixture: ComponentFixture<NumberInputFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ NumberInputFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NumberInputFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
