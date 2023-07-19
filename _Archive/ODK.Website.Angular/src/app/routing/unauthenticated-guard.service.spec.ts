import { TestBed } from '@angular/core/testing';

import { UnauthenticatedGuardService } from './unauthenticated-guard.service';

describe('UnauthenticatedGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: UnauthenticatedGuardService = TestBed.inject(UnauthenticatedGuardService);
    expect(service).toBeTruthy();
  });
});
