import { TestBed } from '@angular/core/testing';

import { ChapterGuardService } from './chapter-guard.service';

describe('ChapterGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ChapterGuardService = TestBed.get(ChapterGuardService);
    expect(service).toBeTruthy();
  });
});
