import { ActivatedRoute, ParamMap } from '@angular/router';

export class RouteUtils {
  static getQueryParam(route: ActivatedRoute, key: string): string {
    const queryParams: ParamMap = route.snapshot.queryParamMap;
    if (queryParams.has(key)) {
      return queryParams.get(key);
    }

    for (const queryParamKey of queryParams.keys) {
      if (queryParamKey.toLocaleLowerCase() === key.toLocaleLowerCase()) {
        return queryParams.get(queryParamKey);
      }
    }
  }
}
