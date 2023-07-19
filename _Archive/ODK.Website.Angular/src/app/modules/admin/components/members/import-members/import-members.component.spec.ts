import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportMembersComponent } from './import-members.component';

describe('ImportMembersComponent', () => {
  let component: ImportMembersComponent;
  let fixture: ComponentFixture<ImportMembersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportMembersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportMembersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
