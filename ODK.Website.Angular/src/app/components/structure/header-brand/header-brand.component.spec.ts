import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HeaderBrandComponent } from './header-brand.component';

describe('HeaderBrandComponent', () => {
  let component: HeaderBrandComponent;
  let fixture: ComponentFixture<HeaderBrandComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ HeaderBrandComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HeaderBrandComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
