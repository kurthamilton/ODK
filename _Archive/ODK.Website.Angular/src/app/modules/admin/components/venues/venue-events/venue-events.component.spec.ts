import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueEventsComponent } from './venue-events.component';

describe('VenueEventsComponent', () => {
  let component: VenueEventsComponent;
  let fixture: ComponentFixture<VenueEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VenueEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VenueEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
