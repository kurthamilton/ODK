import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ThreeTenetsComponent } from './three-tenets.component';

describe('ThreeTenetsComponent', () => {
  let component: ThreeTenetsComponent;
  let fixture: ComponentFixture<ThreeTenetsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ThreeTenetsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ThreeTenetsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
