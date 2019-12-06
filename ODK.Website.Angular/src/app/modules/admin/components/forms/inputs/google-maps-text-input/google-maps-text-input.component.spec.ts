import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GoogleMapsTextInputComponent } from './google-maps-text-input.component';

describe('GoogleMapsTextInputComponent', () => {
  let component: GoogleMapsTextInputComponent;
  let fixture: ComponentFixture<GoogleMapsTextInputComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GoogleMapsTextInputComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GoogleMapsTextInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
