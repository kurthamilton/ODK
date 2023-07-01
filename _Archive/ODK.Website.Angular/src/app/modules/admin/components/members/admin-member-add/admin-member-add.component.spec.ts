import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminMemberAddComponent } from './admin-member-add.component';

describe('AdminMemberAddComponent', () => {
  let component: AdminMemberAddComponent;
  let fixture: ComponentFixture<AdminMemberAddComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AdminMemberAddComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AdminMemberAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
