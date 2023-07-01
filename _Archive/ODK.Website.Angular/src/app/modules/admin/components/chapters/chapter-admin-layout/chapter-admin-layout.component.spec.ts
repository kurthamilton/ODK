import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterAdminLayoutComponent } from './chapter-admin-layout.component';

describe('ChapterAdminLayoutComponent', () => {
  let component: ChapterAdminLayoutComponent;
  let fixture: ComponentFixture<ChapterAdminLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterAdminLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterAdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
