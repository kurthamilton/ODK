import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterPropertyComponent } from './chapter-property.component';

describe('ChapterPropertyComponent', () => {
  let component: ChapterPropertyComponent;
  let fixture: ComponentFixture<ChapterPropertyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterPropertyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterPropertyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
