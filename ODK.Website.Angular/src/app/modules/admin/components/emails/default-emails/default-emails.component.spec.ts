import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DefaultEmailsComponent } from './default-emails.component';

describe('DefaultEmailsComponent', () => {
  let component: DefaultEmailsComponent;
  let fixture: ComponentFixture<DefaultEmailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DefaultEmailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DefaultEmailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
