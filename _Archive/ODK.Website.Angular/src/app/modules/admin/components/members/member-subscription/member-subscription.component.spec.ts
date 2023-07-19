import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberSubscriptionComponent } from './member-subscription.component';

describe('MemberSubscriptionComponent', () => {
  let component: MemberSubscriptionComponent;
  let fixture: ComponentFixture<MemberSubscriptionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberSubscriptionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberSubscriptionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
