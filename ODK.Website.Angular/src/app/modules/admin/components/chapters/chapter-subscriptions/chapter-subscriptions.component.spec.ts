import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSubscriptionsComponent } from './chapter-subscriptions.component';

describe('ChapterSubscriptionsComponent', () => {
  let component: ChapterSubscriptionsComponent;
  let fixture: ComponentFixture<ChapterSubscriptionsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSubscriptionsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSubscriptionsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
