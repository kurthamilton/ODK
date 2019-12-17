import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DropDownMultiComponent } from './drop-down-multi.component';

describe('DropDownMultiComponent', () => {
  let component: DropDownMultiComponent;
  let fixture: ComponentFixture<DropDownMultiComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DropDownMultiComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DropDownMultiComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
