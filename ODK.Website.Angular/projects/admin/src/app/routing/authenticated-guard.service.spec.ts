import { TestBed } from '@angular/core/testing';

import { AuthenticatedGuardService } from './authenticated-guard.service';

describe('AuthenticatedGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AuthenticatedGuardService = TestBed.get(AuthenticatedGuardService);
    expect(service).toBeTruthy();
  });
});
