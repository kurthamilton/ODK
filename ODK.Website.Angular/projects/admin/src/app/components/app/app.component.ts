import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  
  constructor(private authService: AuthenticationService) {

  }

  ngOnInit(): void {
    console.log(this.authService.getToken());
  }

  title = 'admin';  
}
