import { TestBed } from '@angular/core/testing';

import { VenueAdminService } from './venue-admin.service';

describe('VenueAdminService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: VenueAdminService = TestBed.inject(VenueAdminService);
    expect(service).toBeTruthy();
  });
});
