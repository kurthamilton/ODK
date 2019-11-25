import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormControlValidationComponent } from './form-control-validation.component';

describe('FormControlValidationComponent', () => {
  let component: FormControlValidationComponent;
  let fixture: ComponentFixture<FormControlValidationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormControlValidationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormControlValidationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
