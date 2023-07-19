import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditMemberImageComponent } from './edit-member-image.component';

describe('EditMemberImageComponent', () => {
  let component: EditMemberImageComponent;
  let fixture: ComponentFixture<EditMemberImageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditMemberImageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditMemberImageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
