import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MediaFilesComponent } from './media-files.component';

describe('MediaFilesComponent', () => {
  let component: MediaFilesComponent;
  let fixture: ComponentFixture<MediaFilesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MediaFilesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MediaFilesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
