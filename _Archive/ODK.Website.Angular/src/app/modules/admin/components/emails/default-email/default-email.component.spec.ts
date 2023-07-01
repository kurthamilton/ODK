import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DefaultEmailComponent } from './default-email.component';

describe('DefaultEmailComponent', () => {
  let component: DefaultEmailComponent;
  let fixture: ComponentFixture<DefaultEmailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DefaultEmailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DefaultEmailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
