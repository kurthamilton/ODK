import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RawHtmlComponent } from './raw-html.component';

describe('RawHtmlComponent', () => {
  let component: RawHtmlComponent;
  let fixture: ComponentFixture<RawHtmlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RawHtmlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RawHtmlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
