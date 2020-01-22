import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterQuestionFormComponent } from './chapter-question-form.component';

describe('ChapterQuestionFormComponent', () => {
  let component: ChapterQuestionFormComponent;
  let fixture: ComponentFixture<ChapterQuestionFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterQuestionFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterQuestionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
