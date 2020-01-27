import { TestBed } from '@angular/core/testing';

import { MediaAdminService } from './media-admin.service';

describe('MediaAdminService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: MediaAdminService = TestBed.get(MediaAdminService);
    expect(service).toBeTruthy();
  });
});
