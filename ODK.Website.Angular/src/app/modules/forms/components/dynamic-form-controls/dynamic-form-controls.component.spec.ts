import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DynamicFormControlsComponent } from './dynamic-form-controls.component';

describe('DynamicFormControlsComponent', () => {
  let component: DynamicFormControlsComponent;
  let fixture: ComponentFixture<DynamicFormControlsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DynamicFormControlsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DynamicFormControlsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
