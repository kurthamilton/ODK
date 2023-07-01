import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventResponsesComponent } from './event-responses.component';

describe('EventResponsesComponent', () => {
  let component: EventResponsesComponent;
  let fixture: ComponentFixture<EventResponsesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventResponsesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventResponsesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
