import { TestBed } from '@angular/core/testing';

import { ChapterAdminGuardService } from './chapter-admin-guard.service';

describe('ChapterAdminGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ChapterAdminGuardService = TestBed.inject(ChapterAdminGuardService);
    expect(service).toBeTruthy();
  });
});
