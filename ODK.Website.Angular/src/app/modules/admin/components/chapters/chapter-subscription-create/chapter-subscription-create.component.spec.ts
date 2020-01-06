import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterSubscriptionCreateComponent } from './chapter-subscription-create.component';

describe('ChapterSubscriptionCreateComponent', () => {
  let component: ChapterSubscriptionCreateComponent;
  let fixture: ComponentFixture<ChapterSubscriptionCreateComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterSubscriptionCreateComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterSubscriptionCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
