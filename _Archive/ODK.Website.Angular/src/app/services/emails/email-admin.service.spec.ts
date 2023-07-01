import { TestBed } from '@angular/core/testing';

import { EmailAdminService } from './email-admin.service';

describe('EmailAdminService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EmailAdminService = TestBed.inject(EmailAdminService);
    expect(service).toBeTruthy();
  });
});
