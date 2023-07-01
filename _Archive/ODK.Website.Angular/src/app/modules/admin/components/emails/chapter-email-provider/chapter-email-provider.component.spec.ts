import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterEmailProviderComponent } from './chapter-email-provider.component';

describe('ChapterEmailProviderComponent', () => {
  let component: ChapterEmailProviderComponent;
  let fixture: ComponentFixture<ChapterEmailProviderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterEmailProviderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterEmailProviderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
