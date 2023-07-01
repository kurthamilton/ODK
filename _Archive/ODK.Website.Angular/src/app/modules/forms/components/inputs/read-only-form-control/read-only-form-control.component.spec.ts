import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ReadOnlyFormControlComponent } from './read-only-form-control.component';

describe('ReadOnlyFormControlComponent', () => {
  let component: ReadOnlyFormControlComponent;
  let fixture: ComponentFixture<ReadOnlyFormControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ReadOnlyFormControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReadOnlyFormControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
