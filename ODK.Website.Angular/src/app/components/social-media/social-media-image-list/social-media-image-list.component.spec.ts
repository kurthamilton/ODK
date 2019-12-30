import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialMediaImageListComponent } from './social-media-image-list.component';

describe('SocialMediaImageListComponent', () => {
  let component: SocialMediaImageListComponent;
  let fixture: ComponentFixture<SocialMediaImageListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SocialMediaImageListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SocialMediaImageListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
