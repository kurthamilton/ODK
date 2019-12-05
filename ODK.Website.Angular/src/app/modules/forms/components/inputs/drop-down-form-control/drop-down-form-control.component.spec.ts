import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DropDownFormControlComponent } from './drop-down-form-control.component';

describe('DropDownFormControlComponent', () => {
  let component: DropDownFormControlComponent;
  let fixture: ComponentFixture<DropDownFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DropDownFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DropDownFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
