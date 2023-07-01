import { TestBed } from '@angular/core/testing';

import { EventAdminService } from './event-admin.service';

describe('EventService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EventAdminService = TestBed.inject(EventAdminService);
    expect(service).toBeTruthy();
  });
});
