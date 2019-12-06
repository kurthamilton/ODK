import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GoogleMapsTextInputFormControlComponent } from './google-maps-text-input-form-control.component';

describe('GoogleMapsTextInputComponent', () => {
  let component: GoogleMapsTextInputFormControlComponent;
  let fixture: ComponentFixture<GoogleMapsTextInputFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GoogleMapsTextInputFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GoogleMapsTextInputFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
