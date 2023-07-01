import { TestBed } from '@angular/core/testing';

import { ScriptService } from './script.service';

describe('ScriptService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: ScriptService = TestBed.inject(ScriptService);
    expect(service).toBeTruthy();
  });
});
