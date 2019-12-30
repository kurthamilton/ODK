import { ActivatedRoute, ParamMap } from '@angular/router';

export class RouteUtils {
  static getQueryParam(route: ActivatedRoute, key: string): string {
    const queryParams: ParamMap = route.snapshot.queryParamMap;
    if (queryParams.has(key)) {
      return queryParams.get(key);
    }

    for (let i = 0; i < queryParams.keys.length; i++) {
      const queryParamKey: string = queryParams.keys[i];
      if (queryParamKey.toLocaleLowerCase() === key.toLocaleLowerCase()) {
        return queryParams.get(queryParamKey);
      }
    }

    return undefined;
  }
}