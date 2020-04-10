import { TestBed } from '@angular/core/testing';

import { MemberAdminService } from './member-admin.service';

describe('MemberAdminService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: MemberAdminService = TestBed.inject(MemberAdminService);
    expect(service).toBeTruthy();
  });
});
