import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ChapterMembershipSettingsComponent } from './chapter-membership-settings.component';

describe('ChapterMembershipSettingsComponent', () => {
  let component: ChapterMembershipSettingsComponent;
  let fixture: ComponentFixture<ChapterMembershipSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChapterMembershipSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChapterMembershipSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
