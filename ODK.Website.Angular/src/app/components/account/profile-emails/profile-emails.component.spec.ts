import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileEmailsComponent } from './profile-emails.component';

describe('ProfileEmailsComponent', () => {
  let component: ProfileEmailsComponent;
  let fixture: ComponentFixture<ProfileEmailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProfileEmailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfileEmailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
