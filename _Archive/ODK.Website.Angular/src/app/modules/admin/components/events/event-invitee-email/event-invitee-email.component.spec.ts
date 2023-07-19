import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventInviteeEmailComponent } from './event-invitee-email.component';

describe('EventInviteeEmailComponent', () => {
  let component: EventInviteeEmailComponent;
  let fixture: ComponentFixture<EventInviteeEmailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventInviteeEmailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventInviteeEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
