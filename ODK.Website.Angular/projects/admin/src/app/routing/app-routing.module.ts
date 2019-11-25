import { NgModule } from '@angular/core';
import { Routes, RouterModule, UrlSerializer, DefaultUrlSerializer, UrlTree } from '@angular/router';

import { AppComponent } from '../components/app/app.component';
import { AuthenticatedGuardService } from 'src/app/routing/authenticated-guard.service';

const routes: Routes = [
  { path: '', component: AppComponent, canActivate: [AuthenticatedGuardService] }
];

export class LowerCaseUrlSerializer extends DefaultUrlSerializer {
  parse(url: string): UrlTree { 
    return super.parse(url.toLowerCase());
  }
}

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
  providers: [
    { provide: UrlSerializer, useClass: LowerCaseUrlSerializer },
  ]
})
export class AppRoutingModule { }
