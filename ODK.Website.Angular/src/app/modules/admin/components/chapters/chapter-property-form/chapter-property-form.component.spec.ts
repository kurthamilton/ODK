import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterPropertyFormComponent } from './chapter-property-form.component';

describe('ChapterPropertyFormComponent', () => {
  let component: ChapterPropertyFormComponent;
  let fixture: ComponentFixture<ChapterPropertyFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterPropertyFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterPropertyFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
