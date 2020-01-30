import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventsAdminLayoutComponent } from './events-admin-layout.component';

describe('EventsAdminLayoutComponent', () => {
  let component: EventsAdminLayoutComponent;
  let fixture: ComponentFixture<EventsAdminLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventsAdminLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventsAdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
