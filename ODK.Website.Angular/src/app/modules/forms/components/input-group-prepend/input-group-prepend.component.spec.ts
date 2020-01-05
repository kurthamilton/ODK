import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InputGroupPrependComponent } from './input-group-prepend.component';

describe('InputGroupPrependComponent', () => {
  let component: InputGroupPrependComponent;
  let fixture: ComponentFixture<InputGroupPrependComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InputGroupPrependComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InputGroupPrependComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
