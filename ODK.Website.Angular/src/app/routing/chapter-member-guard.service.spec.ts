import { TestBed } from '@angular/core/testing';

import { ChapterMemberGuardService } from './chapter-member-guard.service';

describe('ChapterMemberGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ChapterMemberGuardService = TestBed.get(ChapterMemberGuardService);
    expect(service).toBeTruthy();
  });
});
