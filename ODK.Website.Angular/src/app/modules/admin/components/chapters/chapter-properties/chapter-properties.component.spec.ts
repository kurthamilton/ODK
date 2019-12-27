import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterPropertiesComponent } from './chapter-properties.component';

describe('ChapterPropertiesComponent', () => {
  let component: ChapterPropertiesComponent;
  let fixture: ComponentFixture<ChapterPropertiesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterPropertiesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterPropertiesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
