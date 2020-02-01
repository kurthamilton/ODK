import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSocialLinksComponent } from './chapter-social-links.component';

describe('ChapterSocialLinksComponent', () => {
  let component: ChapterSocialLinksComponent;
  let fixture: ComponentFixture<ChapterSocialLinksComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSocialLinksComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSocialLinksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
