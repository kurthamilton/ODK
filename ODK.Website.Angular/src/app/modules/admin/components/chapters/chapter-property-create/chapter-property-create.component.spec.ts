import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterPropertyCreateComponent } from './chapter-property-create.component';

describe('ChapterPropertyCreateComponent', () => {
  let component: ChapterPropertyCreateComponent;
  let fixture: ComponentFixture<ChapterPropertyCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterPropertyCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterPropertyCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
