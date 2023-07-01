import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterEmailProvidersComponent } from './chapter-email-providers.component';

describe('ChapterEmailProvidersComponent', () => {
  let component: ChapterEmailProvidersComponent;
  let fixture: ComponentFixture<ChapterEmailProvidersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterEmailProvidersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterEmailProvidersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
