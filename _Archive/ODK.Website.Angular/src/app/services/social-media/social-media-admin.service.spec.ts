import { TestBed } from '@angular/core/testing';

import { SocialMediaAdminService } from './social-media-admin.service';

describe('SocialMediaAdminService', () => {
  let service: SocialMediaAdminService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SocialMediaAdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
