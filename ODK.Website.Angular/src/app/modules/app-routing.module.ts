import { NgModule } from '@angular/core';
import { RouterModule, UrlTree, DefaultUrlSerializer, UrlSerializer, PreloadAllModules } from '@angular/router';

import { appRoutes } from '../routing/app-routes';

export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  parse(url: string): UrlTree {
    return super.parse(url.toLowerCase());
  }
}

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, {
    preloadingStrategy: PreloadAllModules
  })],
  exports: [RouterModule],
  providers: [
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer },
  ]
})
export class AppRoutingModule {}
