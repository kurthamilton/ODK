import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterEmailProviderFormComponent } from './chapter-email-provider-form.component';

describe('ChapterEmailProviderFormComponent', () => {
  let component: ChapterEmailProviderFormComponent;
  let fixture: ComponentFixture<ChapterEmailProviderFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterEmailProviderFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterEmailProviderFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
