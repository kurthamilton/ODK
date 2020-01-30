import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberAdminLayoutComponent } from './member-admin-layout.component';

describe('MemberAdminLayoutComponent', () => {
  let component: MemberAdminLayoutComponent;
  let fixture: ComponentFixture<MemberAdminLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberAdminLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberAdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
