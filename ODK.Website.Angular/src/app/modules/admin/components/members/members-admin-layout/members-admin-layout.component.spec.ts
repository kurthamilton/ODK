import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MembersAdminLayoutComponent } from './members-admin-layout.component';

describe('MembersAdminLayoutComponent', () => {
  let component: MembersAdminLayoutComponent;
  let fixture: ComponentFixture<MembersAdminLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MembersAdminLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MembersAdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
