import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterFooterComponent } from './chapter-footer.component';

describe('ChapterFooterComponent', () => {
  let component: ChapterFooterComponent;
  let fixture: ComponentFixture<ChapterFooterComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterFooterComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterFooterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
