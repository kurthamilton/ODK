import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterAdminMemberComponent } from './chapter-admin-member.component';

describe('ChapterAdminMemberComponent', () => {
  let component: ChapterAdminMemberComponent;
  let fixture: ComponentFixture<ChapterAdminMemberComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterAdminMemberComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterAdminMemberComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
