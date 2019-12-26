import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterAdminMembersComponent } from './chapter-admin-members.component';

describe('ChapterAdminMembersComponent', () => {
  let component: ChapterAdminMembersComponent;
  let fixture: ComponentFixture<ChapterAdminMembersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterAdminMembersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterAdminMembersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
