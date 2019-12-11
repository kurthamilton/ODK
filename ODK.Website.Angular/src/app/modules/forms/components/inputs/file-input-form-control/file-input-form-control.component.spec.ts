import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FileInputFormControlComponent } from './file-input-form-control.component';

describe('FileInputFormControlComponent', () => {
  let component: FileInputFormControlComponent;
  let fixture: ComponentFixture<FileInputFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FileInputFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FileInputFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
