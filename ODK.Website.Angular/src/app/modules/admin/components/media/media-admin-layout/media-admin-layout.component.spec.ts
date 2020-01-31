import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MediaAdminLayoutComponent } from './media-admin-layout.component';

describe('MediaAdminLayoutComponent', () => {
  let component: MediaAdminLayoutComponent;
  let fixture: ComponentFixture<MediaAdminLayoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MediaAdminLayoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MediaAdminLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
