import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterPaymentSettingsComponent } from './chapter-payment-settings.component';

describe('ChapterPaymentSettingsComponent', () => {
  let component: ChapterPaymentSettingsComponent;
  let fixture: ComponentFixture<ChapterPaymentSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterPaymentSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterPaymentSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
