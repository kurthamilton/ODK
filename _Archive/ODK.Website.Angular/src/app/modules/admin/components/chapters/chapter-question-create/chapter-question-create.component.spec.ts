import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterQuestionCreateComponent } from './chapter-question-create.component';

describe('ChapterQuestionCreateComponent', () => {
  let component: ChapterQuestionCreateComponent;
  let fixture: ComponentFixture<ChapterQuestionCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterQuestionCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterQuestionCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
