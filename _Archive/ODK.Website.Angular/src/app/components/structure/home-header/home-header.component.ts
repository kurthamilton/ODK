import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-home-header',
  templateUrl: './home-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeHeaderComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
