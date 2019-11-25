import { TestBed } from '@angular/core/testing';

import { DataTypeService } from './data-type.service';

describe('DataTypeService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: DataTypeService = TestBed.get(DataTypeService);
    expect(service).toBeTruthy();
  });
});
