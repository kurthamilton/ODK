import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PageWithSidebarComponent } from './page-with-sidebar.component';

describe('PageWithSidebarComponent', () => {
  let component: PageWithSidebarComponent;
  let fixture: ComponentFixture<PageWithSidebarComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PageWithSidebarComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PageWithSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
