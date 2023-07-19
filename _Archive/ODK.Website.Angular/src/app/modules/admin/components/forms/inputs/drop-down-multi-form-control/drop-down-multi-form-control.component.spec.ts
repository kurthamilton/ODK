import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DropDownMultiFormControlComponent } from './drop-down-multi-form-control.component';

describe('DropDownMultiFormControlComponent', () => {
  let component: DropDownMultiFormControlComponent;
  let fixture: ComponentFixture<DropDownMultiFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DropDownMultiFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DropDownMultiFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
