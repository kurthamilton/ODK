import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterEmailsComponent } from './chapter-emails.component';

describe('ChapterEmailsComponent', () => {
  let component: ChapterEmailsComponent;
  let fixture: ComponentFixture<ChapterEmailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterEmailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterEmailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
