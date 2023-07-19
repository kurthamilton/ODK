import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VenueLayoutComponent } from './venue-layout.component';

describe('VenueLayoutComponent', () => {
  let component: VenueLayoutComponent;
  let fixture: ComponentFixture<VenueLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VenueLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VenueLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
