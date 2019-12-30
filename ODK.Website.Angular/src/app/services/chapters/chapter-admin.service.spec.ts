import { TestBed } from '@angular/core/testing';

import { ChapterAdminService } from './chapter-admin.service';

describe('ChapterService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ChapterAdminService = TestBed.get(ChapterAdminService);
    expect(service).toBeTruthy();
  });
});
