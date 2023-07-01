import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterQuestionComponent } from './chapter-question.component';

describe('ChapterQuestionComponent', () => {
  let component: ChapterQuestionComponent;
  let fixture: ComponentFixture<ChapterQuestionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterQuestionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterQuestionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
