import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FormControlLabelComponent } from './form-control-label.component';

describe('FormControlLabelComponent', () => {
  let component: FormControlLabelComponent;
  let fixture: ComponentFixture<FormControlLabelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FormControlLabelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FormControlLabelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
