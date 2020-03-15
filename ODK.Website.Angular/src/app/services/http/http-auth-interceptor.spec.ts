import { TestBed } from '@angular/core/testing';

import { HttpAuthInterceptor } from './http-auth-interceptor';

describe('HttpAuthInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: HttpAuthInterceptor = TestBed.get(HttpAuthInterceptor);
    expect(service).toBeTruthy();
  });
});
