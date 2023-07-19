import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventSidebarAttendeesComponent } from './event-sidebar-attendees.component';

describe('EventSidebarAttendeesComponent', () => {
  let component: EventSidebarAttendeesComponent;
  let fixture: ComponentFixture<EventSidebarAttendeesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventSidebarAttendeesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventSidebarAttendeesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
