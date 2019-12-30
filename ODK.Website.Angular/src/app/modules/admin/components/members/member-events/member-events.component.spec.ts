import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberEventsComponent } from './member-events.component';

describe('MemberEventsComponent', () => {
  let component: MemberEventsComponent;
  let fixture: ComponentFixture<MemberEventsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberEventsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberEventsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
