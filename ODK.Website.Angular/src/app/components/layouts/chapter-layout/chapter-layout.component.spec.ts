import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterLayoutComponent } from './chapter-layout.component';

describe('ChapterLayoutComponent', () => {
  let component: ChapterLayoutComponent;
  let fixture: ComponentFixture<ChapterLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
