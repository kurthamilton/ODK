import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventResponseIconComponent } from './event-response-icon.component';

describe('EventResponseIconComponent', () => {
  let component: EventResponseIconComponent;
  let fixture: ComponentFixture<EventResponseIconComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventResponseIconComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventResponseIconComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
