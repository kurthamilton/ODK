import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionAlertComponent } from './subscription-alert.component';

describe('SubscriptionAlertComponent', () => {
  let component: SubscriptionAlertComponent;
  let fixture: ComponentFixture<SubscriptionAlertComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SubscriptionAlertComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SubscriptionAlertComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
