import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSettingsComponent } from './chapter-settings.component';

describe('ChapterSettingsComponent', () => {
  let component: ChapterSettingsComponent;
  let fixture: ComponentFixture<ChapterSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
