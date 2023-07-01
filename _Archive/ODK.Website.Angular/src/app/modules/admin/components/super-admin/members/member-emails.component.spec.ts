import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberEmailsComponent } from './member-emails.component';

describe('MemberEmailsComponent', () => {
  let component: MemberEmailsComponent;
  let fixture: ComponentFixture<MemberEmailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberEmailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberEmailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
