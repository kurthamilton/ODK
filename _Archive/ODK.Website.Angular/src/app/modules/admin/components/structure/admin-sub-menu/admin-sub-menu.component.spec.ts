import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminSubMenuComponent } from './admin-sub-menu.component';

describe('AdminSubMenuComponent', () => {
  let component: AdminSubMenuComponent;
  let fixture: ComponentFixture<AdminSubMenuComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AdminSubMenuComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AdminSubMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
