import { NgModule } from '@angular/core';
import { RouterModule, UrlSerializer, PreloadAllModules } from '@angular/router';

import { appRoutes } from './app-routes';
import { LowerCaseUrlSerializer } from './lower-case-url-serializer';

@NgModule({
  imports: [RouterModule.forRoot(appRoutes, {
    preloadingStrategy: PreloadAllModules,   
    paramsInheritanceStrategy: 'always'
  })],
  exports: [RouterModule],
  providers: [
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer },
  ]
})
export class AppRoutingModule {}
