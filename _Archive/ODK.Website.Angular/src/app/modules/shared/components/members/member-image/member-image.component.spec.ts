import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberImageComponent } from './member-image.component';

describe('MemberImageComponent', () => {
  let component: MemberImageComponent;
  let fixture: ComponentFixture<MemberImageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MemberImageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MemberImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
