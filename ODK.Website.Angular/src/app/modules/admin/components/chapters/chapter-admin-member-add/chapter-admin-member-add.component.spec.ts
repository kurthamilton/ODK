import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterAdminMemberAddComponent } from './chapter-admin-member-add.component';

describe('ChapterAdminMemberAddComponent', () => {
  let component: ChapterAdminMemberAddComponent;
  let fixture: ComponentFixture<ChapterAdminMemberAddComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterAdminMemberAddComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterAdminMemberAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
