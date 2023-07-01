import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterEmailComponent } from './chapter-email.component';

describe('ChapterEmailComponent', () => {
  let component: ChapterEmailComponent;
  let fixture: ComponentFixture<ChapterEmailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterEmailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
