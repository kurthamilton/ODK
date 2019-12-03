import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicFormLabelComponent } from './dynamic-form-label.component';

describe('DynamicFormLabelComponent', () => {
  let component: DynamicFormLabelComponent;
  let fixture: ComponentFixture<DynamicFormLabelComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DynamicFormLabelComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicFormLabelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
