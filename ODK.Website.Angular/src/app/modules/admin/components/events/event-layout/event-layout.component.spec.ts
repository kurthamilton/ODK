import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EventLayoutComponent } from './event-layout.component';

describe('EventLayoutComponent', () => {
  let component: EventLayoutComponent;
  let fixture: ComponentFixture<EventLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EventLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EventLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
