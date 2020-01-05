import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSubscriptionEditComponent } from './chapter-subscription-edit.component';

describe('ChapterSubscriptionEditComponent', () => {
  let component: ChapterSubscriptionEditComponent;
  let fixture: ComponentFixture<ChapterSubscriptionEditComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSubscriptionEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSubscriptionEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
