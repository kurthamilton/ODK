import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueMenuComponent } from './venue-menu.component';

describe('VenueMenuComponent', () => {
  let component: VenueMenuComponent;
  let fixture: ComponentFixture<VenueMenuComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VenueMenuComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VenueMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
