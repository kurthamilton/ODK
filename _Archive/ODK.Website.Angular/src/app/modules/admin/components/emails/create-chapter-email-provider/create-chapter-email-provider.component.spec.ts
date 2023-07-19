import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateChapterEmailProviderComponent } from './create-chapter-email-provider.component';

describe('CreateChapterEmailProviderComponent', () => {
  let component: CreateChapterEmailProviderComponent;
  let fixture: ComponentFixture<CreateChapterEmailProviderComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreateChapterEmailProviderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateChapterEmailProviderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
