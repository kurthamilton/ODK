import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSidebarComponent } from './chapter-sidebar.component';

describe('ChapterSidebarComponent', () => {
  let component: ChapterSidebarComponent;
  let fixture: ComponentFixture<ChapterSidebarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSidebarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
