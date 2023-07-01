import { HttpHeaders } from '@angular/common/http';

import { HttpAuthInterceptorOptions } from './http-auth-interceptor-options';

const headerNames = {
  isRetry: 'X-AUTH-INTERCEPTOR-IS-RETRY',
  isTokenRefresh: 'X-AUTH-INTERCEPTOR-IS-TOKEN-REFRESH'
};

export class HttpAuthInterceptorHeaders extends HttpHeaders {

  constructor(options: HttpAuthInterceptorOptions) {
    super(HttpAuthInterceptorHeaders.getHeaders(options));
  }

  static getOptions(headers: HttpHeaders): HttpAuthInterceptorOptions {
    if (!headers) {
      return {};
    }

    return {
      isRetry: !!headers.get(headerNames.isRetry),
      isTokenRefresh: !!headers.get(headerNames.isTokenRefresh)
    };
  }

  static removeInterceptorHeaders(headers: HttpHeaders): HttpHeaders {
    for (const key in headerNames) {
      if (headerNames.hasOwnProperty(key)) {
        headers = headers.delete(headerNames[key]);
      }
    }

    return headers;
  }

  static setInterceptorHeaders(headers: HttpHeaders, options: HttpAuthInterceptorOptions): HttpHeaders {
    if (options.isRetry) {
      headers = headers.append(headerNames.isRetry, 'true');
    }

    if (options.isTokenRefresh) {
      headers = headers.append(headerNames.isTokenRefresh, 'true');
    }

    return headers;
  }

  private static getHeaders(options: HttpAuthInterceptorOptions): {} {
    const headers = {};

    if (!options) {
      return headers;
    }

    if (options.isRetry) {
      headers[headerNames.isRetry] = 'true';
    }

    if (options.isTokenRefresh) {
      headers[headerNames.isTokenRefresh] = 'true';
    }

    return headers;
  }
}
