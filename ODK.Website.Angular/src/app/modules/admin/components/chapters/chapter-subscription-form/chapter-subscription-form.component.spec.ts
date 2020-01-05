import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSubscriptionFormComponent } from './chapter-subscription-form.component';

describe('ChapterSubscriptionFormComponent', () => {
  let component: ChapterSubscriptionFormComponent;
  let fixture: ComponentFixture<ChapterSubscriptionFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSubscriptionFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSubscriptionFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
