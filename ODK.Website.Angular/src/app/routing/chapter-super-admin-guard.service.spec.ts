import { TestBed } from '@angular/core/testing';

import { ChapterSuperAdminGuardService } from './chapter-super-admin-guard.service';

describe('ChapterSuperAdminGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ChapterSuperAdminGuardService = TestBed.get(ChapterSuperAdminGuardService);
    expect(service).toBeTruthy();
  });
});
