import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MembershipSettingsComponent } from './membership-settings.component';

describe('MembershipSettingsComponent', () => {
  let component: MembershipSettingsComponent;
  let fixture: ComponentFixture<MembershipSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MembershipSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MembershipSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
